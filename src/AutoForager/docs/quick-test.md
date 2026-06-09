# AutoForager Quick Test Script

This is a streamlined test procedure to verify core functionality quickly.

## Prerequisites
- New save or test save
- SMAPI console visible
- Generic Mod Config Menu installed (optional but recommended)

## Test Procedure (10 minutes)

### 1. Game Launch (1 min)
1. Start game through SMAPI
2. Watch console for errors
3. Load or create a save

**Expected Result:**
- No errors in console
- Mod loads successfully
- See message: "Loading initial asset data"

### 2. Spring Test (3 min)
**Date: Spring 1**

1. Walk to Cindersap Forest (south of farm)
   - [ ] Wild horseradish appears near player (should pick up automatically)
   - [ ] Spring onions in southwest area pick up
   
2. Walk to any tree
   - [ ] Seeds drop when walking near (if tree has seeds)
   
3. Find artifact spot (worm tile)
   - [ ] Digs automatically when walking near

**Expected Result:** All items picked up, no errors

### 3. Berry Season Test (2 min)
**Date: Spring 15** (use CJB Cheats or console to skip time)

1. Walk to any blackberry/salmonberry bush
   - [ ] Berries harvest automatically when walking near
   
**Expected Result:** Berries collected

### 4. Fruit Tree Test (2 min)
**Date: Spring 28** (so fruit trees have 3 fruits)

1. Plant a cherry tree with CJB Item Spawner
2. Use CJB to instantly grow tree (or wait)
3. Walk near tree
   - [ ] Tree shakes and drops fruit

**Expected Result:** Fruit drops from tree

### 5. Configuration Test (2 min)

1. Press Alt+H
   - [ ] See toggle message in HUD
   - [ ] Mod turns off
   
2. Press Alt+H again
   - [ ] Mod turns back on
   
3. Open GMCM (Esc → Options → Generic Mod Config Menu)
   - [ ] Find AutoForager in list
   - [ ] Open config
   - [ ] Navigate to different pages
   - [ ] All pages load without error

**Expected Result:** Configuration accessible and working

## Success Criteria

✅ All checkboxes ticked
✅ No errors in SMAPI console
✅ Mod toggle works
✅ GMCM menu accessible

## If Tests Fail

1. Check SMAPI console for error messages
2. Verify mod is enabled (Alt+H should show "Activated")
3. Check config file exists: `Mods/AutoForager/config.json`
4. Verify handlers initialized (check console for "Loading initial asset data")
5. Report issue with console log excerpt

## Advanced Testing

For thorough testing, see [testing.md](./testing.md) for the complete checklist.