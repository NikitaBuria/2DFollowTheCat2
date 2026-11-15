using System.Collections.Generic;
using UnityEngine;

// The displayed body frame drives these accents, including mirrored clips and interruptions.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class WandererMotionAccents : MonoBehaviour
{
    [SerializeField] private Sprite[] dashFrames;
    [SerializeField] private Sprite[] attackFrames;

    private readonly Dictionary<Sprite, int> frames = new();
    private readonly LineRenderer[] ribbons = new LineRenderer[8];
    private SpriteRenderer body;
    private Sprite previous;
    private bool previousFlip;

    private void Awake()
    {
        body = GetComponent<SpriteRenderer>();
        for (int i = 0; i < dashFrames.Length; i++) frames.Add(dashFrames[i], i);
        for (int i = 0; i < attackFrames.Length; i++) frames.Add(attackFrames[i], 18 + i);

        for (int i = 0; i < ribbons.Length; i++)
        {
            var child = new GameObject(i == 0 ? "Motion sweep" : "Motion accent");
            child.layer = gameObject.layer;
            child.transform.SetParent(transform, false);
            var line = child.AddComponent<LineRenderer>();
            ribbons[i] = line;
            line.useWorldSpace = false;
            line.sharedMaterial = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
            line.positionCount = i < 3 ? 15 : 3;
            line.widthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(.68f, 1), new Keyframe(1, 0));
            line.enabled = false;
        }
    }

    private void OnDisable()
    {
        previous = null;
        foreach (var line in ribbons)
            if (line != null) line.enabled = false;
    }

    private void LateUpdate()
    {
        var sprite = body.sprite;
        if (sprite == previous && previousFlip == body.flipX) return;
        previous = sprite;
        previousFlip = body.flipX;
        int index = 0;
        bool active = sprite != null && frames.TryGetValue(sprite, out index);
        int pose = index % 6;
        bool strike = index >= 18;
        active &= pose >= (strike ? 2 : 1) && pose <= 5;
        for (int i = 0; i < ribbons.Length; i++)
        {
            var line = ribbons[i];
            line.enabled = active;
            if (!active) continue;
            int row = (index % 18) / 6;
            Vector2 forward = row == 0 ? Vector2.down : row == 1 ? Vector2.up :
                body.flipX ? Vector2.left : Vector2.right;
            Vector2 side = new Vector2(-forward.y, forward.x);
            Vector2 center = sprite.bounds.center;
            if (body.flipX) center.x = -center.x;
            float size = sprite.bounds.size.y;
            line.sortingLayerID = body.sortingLayerID;
            line.sortingOrder = body.sortingOrder + (strike && row != 1 ? 1 : -1);
            float strength = pose == 5 ? .32f : pose == 4 ? .72f : 1f;
            Color tint = strike ? new Color(.98f, .79f, .45f) : new Color(.40f, .84f, .79f);
            if (i == 1 || i == 4) tint = Color.Lerp(tint, Color.white, .82f);
            line.startColor = new Color(tint.r, tint.g, tint.b, .48f * strength);
            line.endColor = new Color(tint.r, tint.g, tint.b, strength);
            line.widthMultiplier = size * strength * (i == 0 ? .105f : i == 1 ? .045f : .029f);
            for (int j = 0; j < line.positionCount; j++)
            {
                float t = j / (float)(line.positionCount - 1);
                Vector2 offset;
                if (i >= 3)
                {
                    offset = BurstOffset(strike, pose, i - 3, t, forward, side);
                }
                else if (strike)
                {
                    float angle = Mathf.Lerp(-1.55f, .65f, t) + (pose - 2) * .53f - i * .22f;
                    offset = (forward * Mathf.Cos(angle) + side * Mathf.Sin(angle)) * (.80f - i * .053f);
                    offset.y *= .84f;
                }
                else
                {
                    float length = 1.06f + .26f * Mathf.Sin(pose * .8f);
                    offset = -forward * Mathf.Lerp(length + i * .10f, .16f, t)
                             + side * ((i == 0 ? -.15f : i == 1 ? -.10f : .21f) + .055f * Mathf.Sin(t * Mathf.PI));
                }
                line.SetPosition(j, center + offset * size);
            }
        }
    }

    // A compact star at the swing tip opens into embers. Dash flecks peel off the wake.
    private static Vector2 BurstOffset(bool strike, int pose, int ray, float t,
        Vector2 forward, Vector2 side)
    {
        float fan = (ray - 2) * .55f;
        if (strike)
        {
            float travel = (pose - 2) * .10f;
            Vector2 tip = forward * .70f + side * .22f;
            Vector2 direction = forward * Mathf.Cos(fan) + side * Mathf.Sin(fan);
            float length = ray % 2 == 0 ? .31f : .18f;
            return tip + direction * (travel + t * length);
        }

        float spread = (ray - 2) * .11f;
        float distance = .52f + (pose - 1) * .10f + (ray % 2) * .20f;
        return -forward * (distance + t * .19f) + side * spread * (1f + t);
    }
}