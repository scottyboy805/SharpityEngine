using SharpityEngine.Content;
using SharpityEngine.Player;

#if DEBUG
string contentPath = "../../../../../../Content";
#else
string contentPath = "Content";
#endif

// Create and run game with SDL2 platform
new SDL2_GamePlatform(
    new FileContentProvider(contentPath))
    .RunAsync(args);

//TestIsolatedRendering.MainTest(args);
