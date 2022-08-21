
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrGfx;

var gfx = Gfx.Open("mr.bcmdl");
var anim = gfx.MaterialAnimations[0];
var h3dAnim = anim.ToH3DAnimation();
anim.FromH3D(h3dAnim);

var h3d = H3D.Open(File.ReadAllBytes("costume_brave.bch"));
H3D.Save(@"C:\Users\Nathan\AppData\Roaming\Citra\load\mods\0004000000176F00\romfs\Common\Costume\costume_brave.bch", h3d);