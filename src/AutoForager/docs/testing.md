# AutoForager Testing Guide

## Overview
This document provides comprehensive testing procedures for AutoForager after the modular refactoring.

## Quick Test (5 minutes)
Run through these basics to verify core functionality:

1. **Load the game** - verify no errors in SMAPI console
2. **Pick up a wild item** (dandelion, leek, etc.)
3. **Shake a berry bush** (salmonberry in Spring 15-18, blackberry in Fall 8-11)
4. **Shake a fruit tree** with fruit on it
5. **Dig an artifact spot**
6. **Open GMCM** - verify menu loads and all pages are accessible

## Full Functional Test Checklist

### Wild Trees (WildTreeHandler)
- [ ] Maple seed drops when shaking
- [ ] Oak seed (acorn) drops when shaking
- [ ] Pine seed (pine cone) drops when shaking
- [ ] Moss harvests from trees (requires tool if config enabled)
- [ ] Moss respects mushroom log exclusion setting
- [ ] No errors with modded trees (if installed)

### Fruit Trees (FruitTreeHandler)
- [ ] Cherry tree shakes (Spring)
- [ ] Apricot tree shakes (Spring)
- [ ] Orange tree shakes (Summer)
- [ ] Peach tree shakes (Summer)
- [ ] Pomegranate tree shakes (Fall)
- [ ] Apple tree shakes (Fall)
- [ ] Banana tree shakes (if on Ginger Island)
- [ ] Mango tree shakes (if on Ginger Island)
- [ ] Respects "Fruits Ready to Shake" setting (1-3)
- [ ] No errors with modded fruit trees (if installed)

### Bushes (BushHandler)
- [ ] Salmonberries harvest (Spring 15-18)
- [ ] Blackberries harvest (Fall 8-11)
- [ ] Tea bushes harvest (last week of each season)
- [ ] Walnut bushes harvest (Ginger Island)
- [ ] Custom bushes harvest (if Custom Bush mod installed)
- [ ] Bush Bloom bushes harvest (if Bush Bloom mod installed)
- [ ] Town bushes are ignored
- [ ] Large bushes work correctly

### Terrain Features (TerrainFeatureHandler)
- [ ] Spring onions harvest (Spring, Cindersap Forest)
- [ ] Ginger roots harvest (Ginger Island)
- [ ] Ginger respects hoe requirement
- [ ] No errors with other forage crops

### Ground Objects (ObjectHandler)
- [ ] Wild horseradish picks up
- [ ] Daffodils pick up
- [ ] Leeks pick up
- [ ] Dandelions pick up
- [ ] All seasonal forageables work
- [ ] Beach forageables work (coral, nautilus shell, etc.)
- [ ] Desert forageables work (cactus fruit, coconut)
- [ ] Gatherer profession doubles items (20% chance)
- [ ] Botanist profession gives iridium quality
- [ ] Quality scales with foraging level

### Artifact & Seed Spots (ArtifactSpotHandler)
- [ ] Artifact spots dig automatically
- [ ] Seed spots dig automatically (if enabled)
- [ ] Respects hoe requirement setting
- [ ] Items spawn correctly
- [ ] No duplication issues

### Machines (MachineHandler)
- [ ] Mushroom boxes harvest
- [ ] Mushroom logs harvest
- [ ] Tappers harvest (maple syrup, oak resin, pine tar)
- [ ] Heavy tappers harvest
- [ ] Experience grants correctly (5 XP)
- [ ] Tapper updates correctly after harvest

### Panning (PanningHandler)
- [ ] Panning spots work
- [ ] Items spawn correctly
- [ ] Respects pan requirement
- [ ] Upgraded pans spawn additional spots
- [ ] Works in all locations with panning spots

## Configuration Testing

### GMCM Integration
- [ ] Menu opens without errors
- [ ] All pages load (General, Wild Trees, Fruit Trees, Bushes, Forageables)
- [ ] Toggles work correctly
- [ ] Changes save and persist
- [ ] Reset to defaults works
- [ ] Keybind changes work

### Config File
- [ ] Settings persist after game restart
- [ ] Manual edits to config.json work
- [ ] Invalid values are clamped/fixed
- [ ] Merge logic works for new settings

### Toggle Keybind
- [ ] Default keybind (Alt+H) toggles mod on/off
- [ ] Custom keybinds work
- [ ] HUD message displays correctly
- [ ] State persists correctly

## Edge Cases & Special Scenarios

### Tool Requirements
- [ ] Hoe requirement works (can't dig without hoe)
- [ ] Hoe requirement bypass works (digs without hoe when disabled)
- [ ] Pan requirement works
- [ ] Tool moss requirement works
- [ ] Error messages display correctly (throttled to 10s)

### Multiplayer
- [ ] Works in multiplayer (farmhands and host)
- [ ] No desync issues
- [ ] Items tracked correctly per player
- [ ] No conflicts with other players

### Performance
- [ ] No lag when walking around
- [ ] No frame drops with many forageables
- [ ] Large radius doesn't cause issues
- [ ] Player magnetism mode works

### Integration with Other Mods
- [ ] Bush Bloom Mod integration works
- [ ] Custom Bush integration works
- [ ] Farm Type Manager integration works
- [ ] Content Patcher forageables work
- [ ] No conflicts with other foraging mods

## Daily Statistics

### End of Day Report
- [ ] Statistics display correctly
- [ ] All categories show (bushes, forageables, fruit trees, wild trees)
- [ ] Counts are accurate
- [ ] No negative values
- [ ] Clears correctly for next day

## Debugging Tips

### Common Issues

**Problem: Nothing is being foraged**
- Check if mod is enabled (Alt+H to toggle)
- Verify UpdateTicked event is subscribed
- Check SMAPI console for errors
- Verify handlers initialized correctly

**Problem: Some items not working**
- Check if item is enabled in GMCM
- Verify item is in forageable tracker
- Check asset editing (object/tree custom fields)
- Review handler CanHandle logic

**Problem: Errors in console**
- Note the handler name in stack trace
- Check if it's handler-specific or general
- Verify context is initialized
- Check for null references

**Problem: GMCM menu doesn't load**
- Verify Generic Mod Config Menu is installed
- Check for errors during registration
- Verify ConfigMenuBuilder initializes
- Check forageable tracker has items

### SMAPI Console Monitoring

Look for these log patterns:
```
[AutoForager] Loading initial asset data
[AutoForager] Parsing Fruit Tree Data
[AutoForager] Parsing Object Data
[AutoForager] Found content pack: ...
[AutoForager] Found Bush Bloom Schedule for: ...
```

### Debug Mode

Enable debug logs in GMCM (Advanced section):
- Set "Elevate Debug Logs" to true
- Restart game
- Check console for detailed handler logs

## Performance Benchmarks

### Expected Performance
- **Radius 2**: <1ms per tick
- **Radius 5**: 1-2ms per tick
- **Radius 10**: 3-5ms per tick
- **Player Magnetism**: Varies (typically 2-8 tiles)

### If Performance Issues
- Reduce shake distance
- Disable unused handlers
- Check for infinite loops
- Profile with performance tools

## Test Results Template
```
Date: YYYY-MM-DD Tester: [Your Name] Game Version: [Stardew Valley Version] SMAPI Version: [Version] Mod Version: [AutoForager Version]
Quick Test: PASS / FAIL Full Test: PASS / FAIL
Issues Found:
1.	[Description]
2.	[Description]
Notes: [Any additional observations]
```

## Automated Testing (Future)

For future development, consider:
- Unit tests for handlers
- Integration tests for services
- Mock contexts for isolated testing
- CI/CD pipeline integration

## Sign-Off

Once all tests pass:
1. Document any known issues
2. Update release notes
3. Create git tag for release
4. Update documentation
5. Create release build