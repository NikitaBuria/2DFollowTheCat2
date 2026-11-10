# Health potion

HealthPotion.asset uses your existing HealthPotionData / IUsable implementation: heals 3 HP, stacks to 10, sells for 5 coins. These values remain editable in the asset Inspector. HealthPotionPickup.prefab contains the existing ItemPickup, a SpriteRenderer and a small trigger collider. Drag it into a level to make the item collectible.

The world sprite and inventory icon share one texture, capped at 128 pixels at import, with point filtering, compression, no mipmaps and no CPU-readable copy. The world sprite canvas spans 0.5 units; the visible bottle is about 0.34 units tall. There are no new runtime scripts, animation components, lights or per-frame effects.

Your hotbar still only selects items. When you implement usage, call Inventory.UseItem(slotIndex, playerGameObject) from an allowed player state. That method removes one only when healing succeeds. Do not call HealthPotionData.Use directly from the UI because that bypasses inventory consumption. No gameplay scripts or input bindings were changed.

Built-in image-generation brief: one small ruby-red health potion, squat glass bottle, cork, aged brass collar, cream cloth tie, muted detailed pixel art matching Lost Wanderer, bold silhouette readable at small sizes, transparent background, no text or decoration. Source PNG is retained; Unity imports a small runtime texture.

Editor checks cover pickup, full-health preservation, healing and one-item consumption, maximum-health clamping and refusal to revive dead users. In-game hotbar input is intentionally not connected or tested.
