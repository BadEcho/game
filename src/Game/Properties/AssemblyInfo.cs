using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("19d8e431-bf6c-46bd-ac7b-4195fa909ea4")]

[assembly: SuppressMessage("Security", 
                           "CA5394:Do not use insecure randomness", 
                           Scope = "member", 
                           Target = "~M:BadEcho.Game.ParticleSystem.GenerateParticle~BadEcho.Game.Particle",
                           Justification = "Random is being used for visual effects purposes, not security.")]

[assembly: SuppressMessage("Design", 
                           "CA1028:Enum Storage should be Int32", 
                           Scope = "type", 
                           Target = "~T:BadEcho.Game.Tiles.TileFlips",
                           Justification = "The TMX map format is an external specification which defines its tile flip flags as unsigned integer bitmask values.")]

[assembly: SuppressMessage("Design", 
                           "CA1045:Do not pass types by reference", 
                           Scope = "member", 
                           Target = "~M:BadEcho.Game.UI.Control.RemeasureIfChanged``1(``0@,``0)",
                           Justification = "The method is not public, and the immeasurable amount of convenience provided by this function vastly outweighs the inconvenience of passing an argument by reference. This is a proper use of ref parameters.")]

[assembly: SuppressMessage("Design", 
                           "CA1045:Do not pass types by reference", 
                           Scope = "member", 
                           Target = "~M:BadEcho.Game.UI.Control.RearrangeIfChanged``1(``0@,``0)",
                           Justification = "The method is not public, and the immeasurable amount of convenience provided by this function vastly outweighs the inconvenience of passing an argument by reference. This is a proper use of ref parameters.")]