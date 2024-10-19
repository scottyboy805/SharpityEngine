using SharpityEngine.Content;
using SharpityEngine_SDL;

// Create and run game with SDL2 platform
new SDL2_GamePlatform(
    new FileContentProvider("Content"))
    .RunAsync(args);

//TestIsolatedRendering.MainTest(args);
