using System.Collections.Generic;
using UnityEngine;

// What a merchant sells. Stock is unlimited, so it is just a list of items; prices live on each ItemData.
[CreateAssetMenu(fileName = "NewShopStock", menuName = "Scriptable Objects/Shop Stock")]
public class ShopStock : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new();

    public IReadOnlyList<ItemData> itemList => items;

    public bool HasItem(ItemData item) => items.Contains(item);
}
