using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Native;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonIOExtensionsTests
{
    #region Lorem Ipsum
    private const string LoremIpsum = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Amet nisl suscipit adipiscing bibendum est ultricies integer quis. Enim neque volutpat ac tincidunt vitae semper quis lectus. Consequat mauris nunc congue nisi vitae suscipit. Amet facilisis magna etiam tempor orci eu lobortis. Tristique et egestas quis ipsum suspendisse ultrices. Malesuada fames ac turpis egestas sed tempus urna et. Sed enim ut sem viverra aliquet. Id donec ultrices tincidunt arcu non sodales neque sodales ut. Morbi tristique senectus et netus et malesuada fames. Cursus mattis molestie a iaculis at erat pellentesque adipiscing. Arcu dictum varius duis at consectetur lorem donec. Vel fringilla est ullamcorper eget nulla facilisi. Viverra tellus in hac habitasse. Dignissim sodales ut eu sem integer vitae justo.

Rhoncus est pellentesque elit ullamcorper dignissim cras. Sem fringilla ut morbi tincidunt augue interdum velit. Nisi est sit amet facilisis magna etiam tempor orci. Interdum posuere lorem ipsum dolor sit amet. Quam adipiscing vitae proin sagittis nisl. Amet consectetur adipiscing elit duis tristique sollicitudin nibh sit amet. Proin sed libero enim sed faucibus turpis in eu. At elementum eu facilisis sed odio. Feugiat in ante metus dictum at tempor. Elit at imperdiet dui accumsan.

Fermentum posuere urna nec tincidunt praesent semper feugiat nibh. Orci a scelerisque purus semper eget duis at. Odio ut sem nulla pharetra. Blandit turpis cursus in hac habitasse platea. Porttitor eget dolor morbi non. Sed enim ut sem viverra aliquet eget sit amet. Est ullamcorper eget nulla facilisi etiam dignissim. Amet justo donec enim diam vulputate ut. Ut eu sem integer vitae justo eget magna fermentum. Eleifend quam adipiscing vitae proin sagittis. Parturient montes nascetur ridiculus mus mauris vitae. Platea dictumst quisque sagittis purus sit amet volutpat consequat. Tempus iaculis urna id volutpat lacus laoreet. Sit amet tellus cras adipiscing enim eu turpis. Eu consequat ac felis donec et. Dignissim convallis aenean et tortor at risus viverra adipiscing.

Purus viverra accumsan in nisl nisi scelerisque. Semper auctor neque vitae tempus quam pellentesque nec nam. Volutpat commodo sed egestas egestas fringilla. Ultricies mi quis hendrerit dolor magna eget. Sagittis vitae et leo duis ut diam quam nulla porttitor. Ultrices neque ornare aenean euismod elementum nisi quis eleifend quam. Vel elit scelerisque mauris pellentesque pulvinar pellentesque habitant morbi. Donec enim diam vulputate ut. Consectetur adipiscing elit ut aliquam purus sit amet luctus. Nunc mattis enim ut tellus elementum sagittis vitae et. Ultrices neque ornare aenean euismod. Hendrerit gravida rutrum quisque non tellus orci ac auctor. Posuere urna nec tincidunt praesent semper feugiat. A scelerisque purus semper eget duis at tellus at urna. Odio morbi quis commodo odio. Lorem ipsum dolor sit amet consectetur adipiscing elit duis tristique. Tincidunt augue interdum velit euismod.

Nibh ipsum consequat nisl vel pretium lectus quam id leo. At in tellus integer feugiat scelerisque varius. Ac odio tempor orci dapibus. Varius vel pharetra vel turpis nunc eget lorem dolor sed. Nunc sed velit dignissim sodales ut eu. Enim praesent elementum facilisis leo vel fringilla est ullamcorper. Molestie at elementum eu facilisis sed odio morbi quis. Cursus euismod quis viverra nibh cras pulvinar mattis nunc sed. Turpis tincidunt id aliquet risus. Tristique senectus et netus et. Pulvinar elementum integer enim neque volutpat ac. Morbi quis commodo odio aenean sed adipiscing diam donec. Aliquam etiam erat velit scelerisque in. Platea dictumst vestibulum rhoncus est pellentesque elit. Mattis enim ut tellus elementum sagittis vitae. Lacus vestibulum sed arcu non odio euismod lacinia at. Dolor purus non enim praesent elementum facilisis leo vel. Sit amet volutpat consequat mauris nunc congue nisi vitae.

Elit pellentesque habitant morbi tristique. Dolor sit amet consectetur adipiscing elit duis. Commodo viverra maecenas accumsan lacus vel facilisis volutpat est velit. Consequat semper viverra nam libero. Nibh venenatis cras sed felis eget. Faucibus scelerisque eleifend donec pretium vulputate. Varius morbi enim nunc faucibus a pellentesque sit. Non enim praesent elementum facilisis leo vel fringilla est ullamcorper. Mattis ullamcorper velit sed ullamcorper morbi tincidunt ornare massa eget. Duis at consectetur lorem donec massa sapien.

Egestas quis ipsum suspendisse ultrices gravida. Purus in massa tempor nec. Tempus iaculis urna id volutpat lacus laoreet non. Vitae turpis massa sed elementum. Dignissim enim sit amet venenatis urna cursus eget. Bibendum neque egestas congue quisque egestas diam in arcu cursus. Amet consectetur adipiscing elit pellentesque habitant morbi. Risus sed vulputate odio ut enim blandit volutpat. Sed euismod nisi porta lorem mollis aliquam ut. Vel orci porta non pulvinar neque. Commodo ullamcorper a lacus vestibulum sed arcu. Lacus viverra vitae congue eu consequat ac. Mi ipsum faucibus vitae aliquet nec ullamcorper. Scelerisque fermentum dui faucibus in ornare quam. Venenatis lectus magna fringilla urna. Iaculis nunc sed augue lacus viverra vitae congue eu consequat. In iaculis nunc sed augue lacus viverra vitae congue. Placerat in egestas erat imperdiet sed euismod nisi. Sed nisi lacus sed viverra tellus in. Tincidunt ornare massa eget egestas purus viverra.

Ac odio tempor orci dapibus ultrices in iaculis. Amet commodo nulla facilisi nullam vehicula. Enim sit amet venenatis urna cursus. Mattis pellentesque id nibh tortor. In fermentum posuere urna nec tincidunt praesent. Justo donec enim diam vulputate ut pharetra sit amet. Felis bibendum ut tristique et egestas quis ipsum suspendisse. Dui faucibus in ornare quam viverra orci sagittis. Diam quis enim lobortis scelerisque. Dis parturient montes nascetur ridiculus mus mauris vitae ultricies. Quisque sagittis purus sit amet. Laoreet id donec ultrices tincidunt arcu non sodales neque sodales.

Arcu vitae elementum curabitur vitae nunc. Sit amet justo donec enim diam vulputate ut pharetra sit. Ut tortor pretium viverra suspendisse potenti nullam ac tortor vitae. Volutpat maecenas volutpat blandit aliquam etiam erat velit scelerisque. Diam sit amet nisl suscipit adipiscing bibendum est ultricies. Cras semper auctor neque vitae tempus quam pellentesque nec nam. Non sodales neque sodales ut etiam sit amet nisl. Nunc sed blandit libero volutpat. Amet nisl suscipit adipiscing bibendum est ultricies integer. Viverra tellus in hac habitasse platea. Duis ut diam quam nulla. Mi bibendum neque egestas congue quisque egestas diam in. Felis imperdiet proin fermentum leo vel orci porta. Tellus rutrum tellus pellentesque eu tincidunt tortor. Aliquet porttitor lacus luctus accumsan tortor. Nisl condimentum id venenatis a condimentum vitae sapien pellentesque habitant. At lectus urna duis convallis convallis. Venenatis cras sed felis eget velit aliquet sagittis id consectetur. Consequat id porta nibh venenatis cras. Eu mi bibendum neque egestas congue quisque egestas diam.

Tincidunt dui ut ornare lectus sit amet est. Quis enim lobortis scelerisque fermentum dui faucibus in ornare quam. Tristique sollicitudin nibh sit amet commodo. Bibendum arcu vitae elementum curabitur vitae nunc sed. Convallis posuere morbi leo urna molestie at elementum. Ante metus dictum at tempor commodo ullamcorper. Varius duis at consectetur lorem donec massa sapien. Facilisis magna etiam tempor orci eu lobortis elementum. Platea dictumst quisque sagittis purus sit amet. Eget gravida cum sociis natoque penatibus et magnis dis. Libero nunc consequat interdum varius. Porttitor leo a diam sollicitudin. Malesuada proin libero nunc consequat interdum. Elementum facilisis leo vel fringilla est ullamcorper eget. Tortor dignissim convallis aenean et tortor at risus viverra adipiscing.

Tortor dignissim convallis aenean et. Quis ipsum suspendisse ultrices gravida dictum. Urna nunc id cursus metus aliquam. Neque egestas congue quisque egestas. Egestas quis ipsum suspendisse ultrices gravida dictum. Viverra justo nec ultrices dui sapien eget mi proin. Lacus luctus accumsan tortor posuere ac ut. Id faucibus nisl tincidunt eget nullam non nisi est sit. Dolor sit amet consectetur adipiscing elit pellentesque habitant morbi tristique. Est ullamcorper eget nulla facilisi etiam. At risus viverra adipiscing at. Fermentum leo vel orci porta non pulvinar. Nulla aliquet porttitor lacus luctus.

Mauris vitae ultricies leo integer malesuada nunc. Molestie at elementum eu facilisis sed odio. Cursus mattis molestie a iaculis. Tellus at urna condimentum mattis pellentesque id nibh. Tortor pretium viverra suspendisse potenti nullam ac. Et netus et malesuada fames ac turpis egestas integer. Maecenas volutpat blandit aliquam etiam erat velit. Dui id ornare arcu odio. Id neque aliquam vestibulum morbi blandit cursus risus. Tellus in hac habitasse platea dictumst vestibulum rhoncus. Leo in vitae turpis massa sed elementum tempus egestas sed. Fusce ut placerat orci nulla pellentesque. Urna id volutpat lacus laoreet non curabitur gravida arcu ac. Sed lectus vestibulum mattis ullamcorper velit sed ullamcorper morbi. Sem integer vitae justo eget magna fermentum. Faucibus pulvinar elementum integer enim neque volutpat ac tincidunt. Suspendisse interdum consectetur libero id faucibus nisl. Phasellus faucibus scelerisque eleifend donec pretium vulputate sapien nec sagittis.

Donec pretium vulputate sapien nec sagittis aliquam malesuada. Mattis ullamcorper velit sed ullamcorper morbi tincidunt ornare massa eget. Dui accumsan sit amet nulla facilisi. Morbi tristique senectus et netus et malesuada fames. Nulla facilisi morbi tempus iaculis urna. Tortor consequat id porta nibh venenatis cras sed. Nec nam aliquam sem et tortor consequat id porta nibh. Tristique risus nec feugiat in fermentum posuere urna nec. Duis at consectetur lorem donec massa sapien faucibus et. Posuere morbi leo urna molestie at elementum eu facilisis. Dolor sed viverra ipsum nunc. Tristique et egestas quis ipsum suspendisse. Natoque penatibus et magnis dis parturient. Quam vulputate dignissim suspendisse in est. Id semper risus in hendrerit gravida rutrum quisque non tellus. At volutpat diam ut venenatis tellus in metus vulputate. Montes nascetur ridiculus mus mauris vitae ultricies leo integer. Eu scelerisque felis imperdiet proin fermentum leo vel orci porta.

In hac habitasse platea dictumst quisque sagittis purus. In ante metus dictum at tempor. Rutrum tellus pellentesque eu tincidunt tortor aliquam nulla facilisi. Neque convallis a cras semper auctor neque vitae. Et ligula ullamcorper malesuada proin. Turpis massa tincidunt dui ut ornare lectus sit amet. Vestibulum lorem sed risus ultricies tristique nulla. Nunc mi ipsum faucibus vitae. Nibh praesent tristique magna sit amet purus gravida quis blandit. Morbi tincidunt augue interdum velit euismod. Ante metus dictum at tempor commodo. Diam maecenas sed enim ut sem viverra aliquet. Netus et malesuada fames ac. Tellus rutrum tellus pellentesque eu tincidunt tortor aliquam nulla facilisi. Malesuada bibendum arcu vitae elementum. Etiam erat velit scelerisque in dictum non consectetur a erat. Arcu risus quis varius quam quisque id diam.

Habitasse platea dictumst quisque sagittis purus sit amet volutpat consequat. Et tortor consequat id porta nibh venenatis cras sed felis. Sem integer vitae justo eget magna fermentum. Bibendum arcu vitae elementum curabitur vitae. Nulla facilisi cras fermentum odio eu feugiat pretium. Risus nec feugiat in fermentum posuere urna nec tincidunt. Consequat ac felis donec et odio pellentesque. Libero enim sed faucibus turpis in eu mi bibendum. Vulputate eu scelerisque felis imperdiet proin fermentum leo vel. Justo nec ultrices dui sapien eget mi proin sed. Odio tempor orci dapibus ultrices in iaculis. Id diam vel quam elementum. Donec adipiscing tristique risus nec feugiat in.

Sollicitudin aliquam ultrices sagittis orci a. Tellus cras adipiscing enim eu turpis. Suspendisse faucibus interdum posuere lorem ipsum dolor sit amet. Et tortor at risus viverra adipiscing. Odio pellentesque diam volutpat commodo sed egestas. Vitae suscipit tellus mauris a diam maecenas sed. Urna nec tincidunt praesent semper feugiat nibh sed pulvinar proin. In pellentesque massa placerat duis ultricies. Aliquet bibendum enim facilisis gravida neque convallis a. At lectus urna duis convallis convallis tellus id interdum velit. Tellus cras adipiscing enim eu turpis egestas pretium aenean. Commodo viverra maecenas accumsan lacus vel. Faucibus interdum posuere lorem ipsum dolor sit amet.

Sem integer vitae justo eget magna fermentum iaculis eu non. Vitae sapien pellentesque habitant morbi tristique senectus et. Enim blandit volutpat maecenas volutpat blandit. Aliquam malesuada bibendum arcu vitae elementum curabitur vitae. Iaculis nunc sed augue lacus viverra vitae congue eu consequat. In massa tempor nec feugiat nisl. Nascetur ridiculus mus mauris vitae ultricies leo integer. Dolor magna eget est lorem ipsum dolor sit amet consectetur. Massa id neque aliquam vestibulum morbi blandit cursus risus at. Pretium viverra suspendisse potenti nullam. Tristique senectus et netus et malesuada. Bibendum enim facilisis gravida neque convallis a cras. Pharetra pharetra massa massa ultricies. Nunc aliquet bibendum enim facilisis gravida neque convallis. Urna neque viverra justo nec. Nulla posuere sollicitudin aliquam ultrices sagittis orci.

Consectetur purus ut faucibus pulvinar elementum integer enim neque volutpat. Elementum facilisis leo vel fringilla. Volutpat lacus laoreet non curabitur gravida arcu ac tortor dignissim. Neque aliquam vestibulum morbi blandit cursus. Ut tellus elementum sagittis vitae et leo duis ut diam. Elit ullamcorper dignissim cras tincidunt lobortis feugiat vivamus at. Et malesuada fames ac turpis egestas. Non odio euismod lacinia at quis risus. Nulla facilisi nullam vehicula ipsum a arcu. Sit amet mattis vulputate enim nulla aliquet porttitor. Eu feugiat pretium nibh ipsum. Mattis enim ut tellus elementum. Aenean et tortor at risus viverra adipiscing.

Habitasse platea dictumst quisque sagittis. Volutpat blandit aliquam etiam erat velit scelerisque in. Consequat semper viverra nam libero justo. Ipsum a arcu cursus vitae. Ridiculus mus mauris vitae ultricies leo integer malesuada nunc. Magna ac placerat vestibulum lectus mauris. Nunc congue nisi vitae suscipit tellus mauris a. Nisi porta lorem mollis aliquam ut porttitor leo a. Bibendum arcu vitae elementum curabitur vitae nunc sed. Ut aliquam purus sit amet luctus venenatis lectus magna fringilla. Commodo odio aenean sed adipiscing diam donec adipiscing. Nunc vel risus commodo viverra maecenas. Suspendisse sed nisi lacus sed viverra tellus in hac habitasse. Commodo odio aenean sed adipiscing diam donec adipiscing tristique risus. Elit pellentesque habitant morbi tristique senectus et netus et malesuada. Quam id leo in vitae turpis. Sollicitudin tempor id eu nisl.

Euismod elementum nisi quis eleifend quam adipiscing vitae. Magna sit amet purus gravida quis. Magna eget est lorem ipsum dolor sit amet consectetur. Proin sed libero enim sed faucibus turpis in eu mi. Vitae elementum curabitur vitae nunc sed velit dignissim sodales ut. Blandit volutpat maecenas volutpat blandit aliquam etiam. Elementum facilisis leo vel fringilla est ullamcorper eget. Massa sed elementum tempus egestas sed sed risus pretium quam. Nunc id cursus metus aliquam. Fringilla est ullamcorper eget nulla facilisi etiam. Duis at tellus at urna. Fames ac turpis egestas maecenas pharetra convallis. Risus ultricies tristique nulla aliquet enim tortor at. Vitae tempus quam pellentesque nec nam aliquam sem et. Sit amet commodo nulla facilisi nullam vehicula. Congue nisi vitae suscipit tellus mauris a diam. Sit amet risus nullam eget felis eget nunc lobortis. Cursus eget nunc scelerisque viverra mauris. Orci eu lobortis elementum nibh tellus molestie.

Vitae elementum curabitur vitae nunc sed. Laoreet suspendisse interdum consectetur libero. Sit amet luctus venenatis lectus magna fringilla urna. Bibendum arcu vitae elementum curabitur. Viverra vitae congue eu consequat. Eu sem integer vitae justo eget magna. Sit amet mattis vulputate enim nulla. Ultrices eros in cursus turpis massa tincidunt dui. Sit amet est placerat in egestas erat. Quis risus sed vulputate odio ut. Consequat ac felis donec et odio pellentesque diam volutpat commodo. Id donec ultrices tincidunt arcu non sodales neque. Senectus et netus et malesuada fames ac turpis.

Viverra maecenas accumsan lacus vel. Dignissim enim sit amet venenatis urna. Quam elementum pulvinar etiam non quam lacus suspendisse faucibus. Risus viverra adipiscing at in. Mollis nunc sed id semper risus in. Sit amet porttitor eget dolor morbi non. Nulla pellentesque dignissim enim sit amet venenatis urna cursus. Ipsum dolor sit amet consectetur. Sem fringilla ut morbi tincidunt augue interdum velit. Facilisis mauris sit amet massa vitae tortor condimentum lacinia quis. Vitae congue eu consequat ac. Pretium quam vulputate dignissim suspendisse.

Duis convallis convallis tellus id interdum velit laoreet. Auctor elit sed vulputate mi. Sit amet mauris commodo quis imperdiet massa tincidunt. Est ullamcorper eget nulla facilisi etiam dignissim. Magna etiam tempor orci eu lobortis elementum nibh. Arcu cursus euismod quis viverra nibh cras. Tortor id aliquet lectus proin nibh nisl. Gravida arcu ac tortor dignissim. Nibh ipsum consequat nisl vel pretium lectus quam id leo. Nunc sed blandit libero volutpat sed cras ornare. Augue ut lectus arcu bibendum at varius vel pharetra. Malesuada proin libero nunc consequat. Ut tortor pretium viverra suspendisse potenti nullam. Ultricies tristique nulla aliquet enim tortor at auctor. Ultrices neque ornare aenean euismod elementum nisi quis. Bibendum at varius vel pharetra vel. Enim diam vulputate ut pharetra sit amet aliquam. Tellus in metus vulputate eu scelerisque felis imperdiet proin fermentum. In fermentum et sollicitudin ac orci phasellus egestas.

Rutrum tellus pellentesque eu tincidunt tortor aliquam. Nunc pulvinar sapien et ligula. Scelerisque in dictum non consectetur. Mauris rhoncus aenean vel elit scelerisque mauris. Mi eget mauris pharetra et ultrices. In egestas erat imperdiet sed euismod. Donec ultrices tincidunt arcu non sodales neque. Eget est lorem ipsum dolor. Vulputate sapien nec sagittis aliquam malesuada bibendum arcu. Sed viverra tellus in hac habitasse platea dictumst vestibulum. Mauris a diam maecenas sed enim ut sem viverra. Natoque penatibus et magnis dis parturient montes. Elementum curabitur vitae nunc sed velit dignissim sodales ut.

Massa massa ultricies mi quis hendrerit dolor magna eget. Libero enim sed faucibus turpis in. Amet nisl suscipit adipiscing bibendum est. Vitae sapien pellentesque habitant morbi tristique senectus et netus. Massa ultricies mi quis hendrerit dolor. Dictumst quisque sagittis purus sit amet. Tincidunt vitae semper quis lectus nulla at volutpat diam. Facilisi cras fermentum odio eu feugiat pretium nibh ipsum. Neque sodales ut etiam sit. Orci eu lobortis elementum nibh tellus. Eu lobortis elementum nibh tellus molestie nunc non blandit massa. Ac auctor augue mauris augue neque gravida in. Id ornare arcu odio ut sem nulla. Lacus sed viverra tellus in hac habitasse platea dictumst. Congue nisi vitae suscipit tellus mauris a diam. Vitae proin sagittis nisl rhoncus mattis rhoncus.
Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Amet nisl suscipit adipiscing bibendum est ultricies integer quis. Enim neque volutpat ac tincidunt vitae semper quis lectus. Consequat mauris nunc congue nisi vitae suscipit. Amet facilisis magna etiam tempor orci eu lobortis. Tristique et egestas quis ipsum suspendisse ultrices. Malesuada fames ac turpis egestas sed tempus urna et. Sed enim ut sem viverra aliquet. Id donec ultrices tincidunt arcu non sodales neque sodales ut. Morbi tristique senectus et netus et malesuada fames. Cursus mattis molestie a iaculis at erat pellentesque adipiscing. Arcu dictum varius duis at consectetur lorem donec. Vel fringilla est ullamcorper eget nulla facilisi. Viverra tellus in hac habitasse. Dignissim sodales ut eu sem integer vitae justo.

Rhoncus est pellentesque elit ullamcorper dignissim cras. Sem fringilla ut morbi tincidunt augue interdum velit. Nisi est sit amet facilisis magna etiam tempor orci. Interdum posuere lorem ipsum dolor sit amet. Quam adipiscing vitae proin sagittis nisl. Amet consectetur adipiscing elit duis tristique sollicitudin nibh sit amet. Proin sed libero enim sed faucibus turpis in eu. At elementum eu facilisis sed odio. Feugiat in ante metus dictum at tempor. Elit at imperdiet dui accumsan.

Fermentum posuere urna nec tincidunt praesent semper feugiat nibh. Orci a scelerisque purus semper eget duis at. Odio ut sem nulla pharetra. Blandit turpis cursus in hac habitasse platea. Porttitor eget dolor morbi non. Sed enim ut sem viverra aliquet eget sit amet. Est ullamcorper eget nulla facilisi etiam dignissim. Amet justo donec enim diam vulputate ut. Ut eu sem integer vitae justo eget magna fermentum. Eleifend quam adipiscing vitae proin sagittis. Parturient montes nascetur ridiculus mus mauris vitae. Platea dictumst quisque sagittis purus sit amet volutpat consequat. Tempus iaculis urna id volutpat lacus laoreet. Sit amet tellus cras adipiscing enim eu turpis. Eu consequat ac felis donec et. Dignissim convallis aenean et tortor at risus viverra adipiscing.

Purus viverra accumsan in nisl nisi scelerisque. Semper auctor neque vitae tempus quam pellentesque nec nam. Volutpat commodo sed egestas egestas fringilla. Ultricies mi quis hendrerit dolor magna eget. Sagittis vitae et leo duis ut diam quam nulla porttitor. Ultrices neque ornare aenean euismod elementum nisi quis eleifend quam. Vel elit scelerisque mauris pellentesque pulvinar pellentesque habitant morbi. Donec enim diam vulputate ut. Consectetur adipiscing elit ut aliquam purus sit amet luctus. Nunc mattis enim ut tellus elementum sagittis vitae et. Ultrices neque ornare aenean euismod. Hendrerit gravida rutrum quisque non tellus orci ac auctor. Posuere urna nec tincidunt praesent semper feugiat. A scelerisque purus semper eget duis at tellus at urna. Odio morbi quis commodo odio. Lorem ipsum dolor sit amet consectetur adipiscing elit duis tristique. Tincidunt augue interdum velit euismod.

Nibh ipsum consequat nisl vel pretium lectus quam id leo. At in tellus integer feugiat scelerisque varius. Ac odio tempor orci dapibus. Varius vel pharetra vel turpis nunc eget lorem dolor sed. Nunc sed velit dignissim sodales ut eu. Enim praesent elementum facilisis leo vel fringilla est ullamcorper. Molestie at elementum eu facilisis sed odio morbi quis. Cursus euismod quis viverra nibh cras pulvinar mattis nunc sed. Turpis tincidunt id aliquet risus. Tristique senectus et netus et. Pulvinar elementum integer enim neque volutpat ac. Morbi quis commodo odio aenean sed adipiscing diam donec. Aliquam etiam erat velit scelerisque in. Platea dictumst vestibulum rhoncus est pellentesque elit. Mattis enim ut tellus elementum sagittis vitae. Lacus vestibulum sed arcu non odio euismod lacinia at. Dolor purus non enim praesent elementum facilisis leo vel. Sit amet volutpat consequat mauris nunc congue nisi vitae.

Elit pellentesque habitant morbi tristique. Dolor sit amet consectetur adipiscing elit duis. Commodo viverra maecenas accumsan lacus vel facilisis volutpat est velit. Consequat semper viverra nam libero. Nibh venenatis cras sed felis eget. Faucibus scelerisque eleifend donec pretium vulputate. Varius morbi enim nunc faucibus a pellentesque sit. Non enim praesent elementum facilisis leo vel fringilla est ullamcorper. Mattis ullamcorper velit sed ullamcorper morbi tincidunt ornare massa eget. Duis at consectetur lorem donec massa sapien.

Egestas quis ipsum suspendisse ultrices gravida. Purus in massa tempor nec. Tempus iaculis urna id volutpat lacus laoreet non. Vitae turpis massa sed elementum. Dignissim enim sit amet venenatis urna cursus eget. Bibendum neque egestas congue quisque egestas diam in arcu cursus. Amet consectetur adipiscing elit pellentesque habitant morbi. Risus sed vulputate odio ut enim blandit volutpat. Sed euismod nisi porta lorem mollis aliquam ut. Vel orci porta non pulvinar neque. Commodo ullamcorper a lacus vestibulum sed arcu. Lacus viverra vitae congue eu consequat ac. Mi ipsum faucibus vitae aliquet nec ullamcorper. Scelerisque fermentum dui faucibus in ornare quam. Venenatis lectus magna fringilla urna. Iaculis nunc sed augue lacus viverra vitae congue eu consequat. In iaculis nunc sed augue lacus viverra vitae congue. Placerat in egestas erat imperdiet sed euismod nisi. Sed nisi lacus sed viverra tellus in. Tincidunt ornare massa eget egestas purus viverra.

Ac odio tempor orci dapibus ultrices in iaculis. Amet commodo nulla facilisi nullam vehicula. Enim sit amet venenatis urna cursus. Mattis pellentesque id nibh tortor. In fermentum posuere urna nec tincidunt praesent. Justo donec enim diam vulputate ut pharetra sit amet. Felis bibendum ut tristique et egestas quis ipsum suspendisse. Dui faucibus in ornare quam viverra orci sagittis. Diam quis enim lobortis scelerisque. Dis parturient montes nascetur ridiculus mus mauris vitae ultricies. Quisque sagittis purus sit amet. Laoreet id donec ultrices tincidunt arcu non sodales neque sodales.

Arcu vitae elementum curabitur vitae nunc. Sit amet justo donec enim diam vulputate ut pharetra sit. Ut tortor pretium viverra suspendisse potenti nullam ac tortor vitae. Volutpat maecenas volutpat blandit aliquam etiam erat velit scelerisque. Diam sit amet nisl suscipit adipiscing bibendum est ultricies. Cras semper auctor neque vitae tempus quam pellentesque nec nam. Non sodales neque sodales ut etiam sit amet nisl. Nunc sed blandit libero volutpat. Amet nisl suscipit adipiscing bibendum est ultricies integer. Viverra tellus in hac habitasse platea. Duis ut diam quam nulla. Mi bibendum neque egestas congue quisque egestas diam in. Felis imperdiet proin fermentum leo vel orci porta. Tellus rutrum tellus pellentesque eu tincidunt tortor. Aliquet porttitor lacus luctus accumsan tortor. Nisl condimentum id venenatis a condimentum vitae sapien pellentesque habitant. At lectus urna duis convallis convallis. Venenatis cras sed felis eget velit aliquet sagittis id consectetur. Consequat id porta nibh venenatis cras. Eu mi bibendum neque egestas congue quisque egestas diam.

Tincidunt dui ut ornare lectus sit amet est. Quis enim lobortis scelerisque fermentum dui faucibus in ornare quam. Tristique sollicitudin nibh sit amet commodo. Bibendum arcu vitae elementum curabitur vitae nunc sed. Convallis posuere morbi leo urna molestie at elementum. Ante metus dictum at tempor commodo ullamcorper. Varius duis at consectetur lorem donec massa sapien. Facilisis magna etiam tempor orci eu lobortis elementum. Platea dictumst quisque sagittis purus sit amet. Eget gravida cum sociis natoque penatibus et magnis dis. Libero nunc consequat interdum varius. Porttitor leo a diam sollicitudin. Malesuada proin libero nunc consequat interdum. Elementum facilisis leo vel fringilla est ullamcorper eget. Tortor dignissim convallis aenean et tortor at risus viverra adipiscing.

Tortor dignissim convallis aenean et. Quis ipsum suspendisse ultrices gravida dictum. Urna nunc id cursus metus aliquam. Neque egestas congue quisque egestas. Egestas quis ipsum suspendisse ultrices gravida dictum. Viverra justo nec ultrices dui sapien eget mi proin. Lacus luctus accumsan tortor posuere ac ut. Id faucibus nisl tincidunt eget nullam non nisi est sit. Dolor sit amet consectetur adipiscing elit pellentesque habitant morbi tristique. Est ullamcorper eget nulla facilisi etiam. At risus viverra adipiscing at. Fermentum leo vel orci porta non pulvinar. Nulla aliquet porttitor lacus luctus.

Mauris vitae ultricies leo integer malesuada nunc. Molestie at elementum eu facilisis sed odio. Cursus mattis molestie a iaculis. Tellus at urna condimentum mattis pellentesque id nibh. Tortor pretium viverra suspendisse potenti nullam ac. Et netus et malesuada fames ac turpis egestas integer. Maecenas volutpat blandit aliquam etiam erat velit. Dui id ornare arcu odio. Id neque aliquam vestibulum morbi blandit cursus risus. Tellus in hac habitasse platea dictumst vestibulum rhoncus. Leo in vitae turpis massa sed elementum tempus egestas sed. Fusce ut placerat orci nulla pellentesque. Urna id volutpat lacus laoreet non curabitur gravida arcu ac. Sed lectus vestibulum mattis ullamcorper velit sed ullamcorper morbi. Sem integer vitae justo eget magna fermentum. Faucibus pulvinar elementum integer enim neque volutpat ac tincidunt. Suspendisse interdum consectetur libero id faucibus nisl. Phasellus faucibus scelerisque eleifend donec pretium vulputate sapien nec sagittis.

Donec pretium vulputate sapien nec sagittis aliquam malesuada. Mattis ullamcorper velit sed ullamcorper morbi tincidunt ornare massa eget. Dui accumsan sit amet nulla facilisi. Morbi tristique senectus et netus et malesuada fames. Nulla facilisi morbi tempus iaculis urna. Tortor consequat id porta nibh venenatis cras sed. Nec nam aliquam sem et tortor consequat id porta nibh. Tristique risus nec feugiat in fermentum posuere urna nec. Duis at consectetur lorem donec massa sapien faucibus et. Posuere morbi leo urna molestie at elementum eu facilisis. Dolor sed viverra ipsum nunc. Tristique et egestas quis ipsum suspendisse. Natoque penatibus et magnis dis parturient. Quam vulputate dignissim suspendisse in est. Id semper risus in hendrerit gravida rutrum quisque non tellus. At volutpat diam ut venenatis tellus in metus vulputate. Montes nascetur ridiculus mus mauris vitae ultricies leo integer. Eu scelerisque felis imperdiet proin fermentum leo vel orci porta.

In hac habitasse platea dictumst quisque sagittis purus. In ante metus dictum at tempor. Rutrum tellus pellentesque eu tincidunt tortor aliquam nulla facilisi. Neque convallis a cras semper auctor neque vitae. Et ligula ullamcorper malesuada proin. Turpis massa tincidunt dui ut ornare lectus sit amet. Vestibulum lorem sed risus ultricies tristique nulla. Nunc mi ipsum faucibus vitae. Nibh praesent tristique magna sit amet purus gravida quis blandit. Morbi tincidunt augue interdum velit euismod. Ante metus dictum at tempor commodo. Diam maecenas sed enim ut sem viverra aliquet. Netus et malesuada fames ac. Tellus rutrum tellus pellentesque eu tincidunt tortor aliquam nulla facilisi. Malesuada bibendum arcu vitae elementum. Etiam erat velit scelerisque in dictum non consectetur a erat. Arcu risus quis varius quam quisque id diam.

Habitasse platea dictumst quisque sagittis purus sit amet volutpat consequat. Et tortor consequat id porta nibh venenatis cras sed felis. Sem integer vitae justo eget magna fermentum. Bibendum arcu vitae elementum curabitur vitae. Nulla facilisi cras fermentum odio eu feugiat pretium. Risus nec feugiat in fermentum posuere urna nec tincidunt. Consequat ac felis donec et odio pellentesque. Libero enim sed faucibus turpis in eu mi bibendum. Vulputate eu scelerisque felis imperdiet proin fermentum leo vel. Justo nec ultrices dui sapien eget mi proin sed. Odio tempor orci dapibus ultrices in iaculis. Id diam vel quam elementum. Donec adipiscing tristique risus nec feugiat in.

Sollicitudin aliquam ultrices sagittis orci a. Tellus cras adipiscing enim eu turpis. Suspendisse faucibus interdum posuere lorem ipsum dolor sit amet. Et tortor at risus viverra adipiscing. Odio pellentesque diam volutpat commodo sed egestas. Vitae suscipit tellus mauris a diam maecenas sed. Urna nec tincidunt praesent semper feugiat nibh sed pulvinar proin. In pellentesque massa placerat duis ultricies. Aliquet bibendum enim facilisis gravida neque convallis a. At lectus urna duis convallis convallis tellus id interdum velit. Tellus cras adipiscing enim eu turpis egestas pretium aenean. Commodo viverra maecenas accumsan lacus vel. Faucibus interdum posuere lorem ipsum dolor sit amet.

Sem integer vitae justo eget magna fermentum iaculis eu non. Vitae sapien pellentesque habitant morbi tristique senectus et. Enim blandit volutpat maecenas volutpat blandit. Aliquam malesuada bibendum arcu vitae elementum curabitur vitae. Iaculis nunc sed augue lacus viverra vitae congue eu consequat. In massa tempor nec feugiat nisl. Nascetur ridiculus mus mauris vitae ultricies leo integer. Dolor magna eget est lorem ipsum dolor sit amet consectetur. Massa id neque aliquam vestibulum morbi blandit cursus risus at. Pretium viverra suspendisse potenti nullam. Tristique senectus et netus et malesuada. Bibendum enim facilisis gravida neque convallis a cras. Pharetra pharetra massa massa ultricies. Nunc aliquet bibendum enim facilisis gravida neque convallis. Urna neque viverra justo nec. Nulla posuere sollicitudin aliquam ultrices sagittis orci.

Consectetur purus ut faucibus pulvinar elementum integer enim neque volutpat. Elementum facilisis leo vel fringilla. Volutpat lacus laoreet non curabitur gravida arcu ac tortor dignissim. Neque aliquam vestibulum morbi blandit cursus. Ut tellus elementum sagittis vitae et leo duis ut diam. Elit ullamcorper dignissim cras tincidunt lobortis feugiat vivamus at. Et malesuada fames ac turpis egestas. Non odio euismod lacinia at quis risus. Nulla facilisi nullam vehicula ipsum a arcu. Sit amet mattis vulputate enim nulla aliquet porttitor. Eu feugiat pretium nibh ipsum. Mattis enim ut tellus elementum. Aenean et tortor at risus viverra adipiscing.

Habitasse platea dictumst quisque sagittis. Volutpat blandit aliquam etiam erat velit scelerisque in. Consequat semper viverra nam libero justo. Ipsum a arcu cursus vitae. Ridiculus mus mauris vitae ultricies leo integer malesuada nunc. Magna ac placerat vestibulum lectus mauris. Nunc congue nisi vitae suscipit tellus mauris a. Nisi porta lorem mollis aliquam ut porttitor leo a. Bibendum arcu vitae elementum curabitur vitae nunc sed. Ut aliquam purus sit amet luctus venenatis lectus magna fringilla. Commodo odio aenean sed adipiscing diam donec adipiscing. Nunc vel risus commodo viverra maecenas. Suspendisse sed nisi lacus sed viverra tellus in hac habitasse. Commodo odio aenean sed adipiscing diam donec adipiscing tristique risus. Elit pellentesque habitant morbi tristique senectus et netus et malesuada. Quam id leo in vitae turpis. Sollicitudin tempor id eu nisl.

Euismod elementum nisi quis eleifend quam adipiscing vitae. Magna sit amet purus gravida quis. Magna eget est lorem ipsum dolor sit amet consectetur. Proin sed libero enim sed faucibus turpis in eu mi. Vitae elementum curabitur vitae nunc sed velit dignissim sodales ut. Blandit volutpat maecenas volutpat blandit aliquam etiam. Elementum facilisis leo vel fringilla est ullamcorper eget. Massa sed elementum tempus egestas sed sed risus pretium quam. Nunc id cursus metus aliquam. Fringilla est ullamcorper eget nulla facilisi etiam. Duis at tellus at urna. Fames ac turpis egestas maecenas pharetra convallis. Risus ultricies tristique nulla aliquet enim tortor at. Vitae tempus quam pellentesque nec nam aliquam sem et. Sit amet commodo nulla facilisi nullam vehicula. Congue nisi vitae suscipit tellus mauris a diam. Sit amet risus nullam eget felis eget nunc lobortis. Cursus eget nunc scelerisque viverra mauris. Orci eu lobortis elementum nibh tellus molestie.

Vitae elementum curabitur vitae nunc sed. Laoreet suspendisse interdum consectetur libero. Sit amet luctus venenatis lectus magna fringilla urna. Bibendum arcu vitae elementum curabitur. Viverra vitae congue eu consequat. Eu sem integer vitae justo eget magna. Sit amet mattis vulputate enim nulla. Ultrices eros in cursus turpis massa tincidunt dui. Sit amet est placerat in egestas erat. Quis risus sed vulputate odio ut. Consequat ac felis donec et odio pellentesque diam volutpat commodo. Id donec ultrices tincidunt arcu non sodales neque. Senectus et netus et malesuada fames ac turpis.

Viverra maecenas accumsan lacus vel. Dignissim enim sit amet venenatis urna. Quam elementum pulvinar etiam non quam lacus suspendisse faucibus. Risus viverra adipiscing at in. Mollis nunc sed id semper risus in. Sit amet porttitor eget dolor morbi non. Nulla pellentesque dignissim enim sit amet venenatis urna cursus. Ipsum dolor sit amet consectetur. Sem fringilla ut morbi tincidunt augue interdum velit. Facilisis mauris sit amet massa vitae tortor condimentum lacinia quis. Vitae congue eu consequat ac. Pretium quam vulputate dignissim suspendisse.

Duis convallis convallis tellus id interdum velit laoreet. Auctor elit sed vulputate mi. Sit amet mauris commodo quis imperdiet massa tincidunt. Est ullamcorper eget nulla facilisi etiam dignissim. Magna etiam tempor orci eu lobortis elementum nibh. Arcu cursus euismod quis viverra nibh cras. Tortor id aliquet lectus proin nibh nisl. Gravida arcu ac tortor dignissim. Nibh ipsum consequat nisl vel pretium lectus quam id leo. Nunc sed blandit libero volutpat sed cras ornare. Augue ut lectus arcu bibendum at varius vel pharetra. Malesuada proin libero nunc consequat. Ut tortor pretium viverra suspendisse potenti nullam. Ultricies tristique nulla aliquet enim tortor at auctor. Ultrices neque ornare aenean euismod elementum nisi quis. Bibendum at varius vel pharetra vel. Enim diam vulputate ut pharetra sit amet aliquam. Tellus in metus vulputate eu scelerisque felis imperdiet proin fermentum. In fermentum et sollicitudin ac orci phasellus egestas.

Rutrum tellus pellentesque eu tincidunt tortor aliquam. Nunc pulvinar sapien et ligula. Scelerisque in dictum non consectetur. Mauris rhoncus aenean vel elit scelerisque mauris. Mi eget mauris pharetra et ultrices. In egestas erat imperdiet sed euismod. Donec ultrices tincidunt arcu non sodales neque. Eget est lorem ipsum dolor. Vulputate sapien nec sagittis aliquam malesuada bibendum arcu. Sed viverra tellus in hac habitasse platea dictumst vestibulum. Mauris a diam maecenas sed enim ut sem viverra. Natoque penatibus et magnis dis parturient montes. Elementum curabitur vitae nunc sed velit dignissim sodales ut.

Massa massa ultricies mi quis hendrerit dolor magna eget. Libero enim sed faucibus turpis in. Amet nisl suscipit adipiscing bibendum est. Vitae sapien pellentesque habitant morbi tristique senectus et netus. Massa ultricies mi quis hendrerit dolor. Dictumst quisque sagittis purus sit amet. Tincidunt vitae semper quis lectus nulla at volutpat diam. Facilisi cras fermentum odio eu feugiat pretium nibh ipsum. Neque sodales ut etiam sit. Orci eu lobortis elementum nibh tellus. Eu lobortis elementum nibh tellus molestie nunc non blandit massa. Ac auctor augue mauris augue neque gravida in. Id ornare arcu odio ut sem nulla. Lacus sed viverra tellus in hac habitasse platea dictumst. Congue nisi vitae suscipit tellus mauris a diam. Vitae proin sagittis nisl rhoncus mattis rhoncus.";
    private static readonly string[] _expected = ["C:", "Some", "Path"];
    #endregion

    [TestMethod]
    public void CopyToStreamShouldCopyDataCorrectly()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act
        int bytesCopied = sourceStream.CopyToStream(destinationStream, testData.Length);

        // Assert
        Assert.AreEqual(testData.Length, bytesCopied);
        destinationStream.Position = 0;
        using StreamReader reader = new(destinationStream);
        string result = reader.ReadToEnd();
        Assert.AreEqual("Hello, World!", result);

        // Act
        sourceStream.Position = 0;
        destinationStream.Position = 0;
        bytesCopied = sourceStream.CopyToStream(destinationStream, 5);

        // Assert
        Assert.AreEqual(5, bytesCopied);
        destinationStream.Position = 0;
        using StreamReader reader2 = new(destinationStream);
        result = reader2.ReadToEnd();
        Assert.AreEqual("Hello, World!", result);
    }

    [TestMethod]
    public void CopyToStreamShouldReturnZeroWhenSourceStreamIsEmpty()
    {
        // Arrange
        string testData = "";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act
        int bytesCopied = sourceStream.CopyToStream(destinationStream, testData.Length);

        // Assert
        Assert.AreEqual(0, bytesCopied);
    }

    [TestMethod]
    public void CopyToStreamShouldThrowArgumentExceptionWhenDestinationStreamIsReadOnly()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new([], false); // Read-only stream

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => sourceStream.CopyToStream(destinationStream, testData.Length));
    }

    [TestMethod]
    public void CopyToStreamShouldCopyDataInChunks()
    {
        // Arrange
        string testData = "Exercitationem fugiat voluptatum est adipisci. Quia doloribus inventore explicabo quaerat. Et esse facilis esse et qui in non aut.\n\nEos qui repudiandae aut nesciunt et voluptatem deleniti debitis. At magnam ea dolorem ea veritatis. Minus ab eos id laudantium cumque dolorum.";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();
        byte[] buffer = new byte[75]; // Buffer size is 1/4 the length of the text

        // Act
        int bytesCopied = sourceStream.CopyToStream(destinationStream, testData.Length, buffer);

        // Assert
        Assert.AreEqual(testData.Length, bytesCopied);
        destinationStream.Position = 0;
        using StreamReader reader = new(destinationStream);
        string result = reader.ReadToEnd();
        Assert.AreEqual(testData, result);
    }

    [TestMethod]
    public async Task CopyToStreamAsyncShouldCopyDataCorrectly()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act
        int bytesCopied = await sourceStream.CopyToStreamAsync(destinationStream, testData.Length);

        // Assert
        Assert.AreEqual(testData.Length, bytesCopied);
        destinationStream.Position = 0;
        using StreamReader reader = new(destinationStream);
        string result = reader.ReadToEnd();
        Assert.AreEqual(testData, result);
    }

    [TestMethod]
    public async Task CopyToStreamAsyncShouldReturnZeroWhenSourceStreamIsEmpty()
    {
        // Arrange
        string testData = "";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act
        int bytesCopied = await sourceStream.CopyToStreamAsync(destinationStream, testData.Length);

        // Assert
        Assert.AreEqual(0, bytesCopied);
    }

    [TestMethod]
    public async Task CopyToStreamAsyncShouldThrowArgumentExceptionWhenDestinationStreamIsReadOnly()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new([], false); // Read-only stream

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => sourceStream.CopyToStreamAsync(destinationStream, testData.Length));
    }

    [TestMethod]
    public async Task CopyToStreamAsyncShouldThrowArgumentExceptionWhenBufferSizeIsLessThanOne()
    {
        // Arrange
        string testData = "Hello, World!";
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => sourceStream.CopyToStreamAsync(destinationStream, testData.Length, 0));
    }

    /* This test is disabled because we can't reliably test cancellation in a unit test.
    [TestMethod]
    public async Task CopyToStreamAsync_ShouldCancelOperation_WhenCancellationRequested()
    {
        // Arrange
        string testData = new string('a', 100_000_000); // Large data to ensure operation takes some time
        using MemoryStream sourceStream = new(Encoding.UTF8.GetBytes(testData));
        using MemoryStream destinationStream = new();
        CancellationTokenSource cancellationTokenSource = new();

        // Act
        cancellationTokenSource.CancelAfter(20); // Cancel after 10 milliseconds
        try
        {
            await Task.Delay(18);
            await sourceStream.CopyToStreamAsync(destinationStream, testData.Length, 8, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Assert
            Assert.IsTrue(cancellationTokenSource.IsCancellationRequested);
            return;
        }

        Assert.Fail("Expected OperationCanceledException was not thrown.");
    }
    */

    [TestMethod]
    public unsafe void WriteFromPointerWritesCorrectData()
    {
        // Arrange
        byte[] data = Encoding.ASCII.GetBytes("Test a string that we can write to a stream.");
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        fixed (byte* pData = data)
        {
            GorgonPtr<byte> srcBuffer = new(pData, data.Length);

            // Act
            writer.WriteFromPointer(srcBuffer);

            // Assert
            byte[] writtenData = stream.ToArray();
            Assert.IsTrue(data.SequenceEqual(writtenData));
        }
    }

    [TestMethod]
    public void WriteFromPointerDoesNothingWhenPointerIsNull()
    {
        // Arrange
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        GorgonPtr<byte> nullPointer = GorgonPtr<byte>.NullPtr;

        // Act
        writer.WriteFromPointer(nullPointer);

        // Assert
        Assert.AreEqual(0, stream.Length);
    }

    [TestMethod]
    public unsafe void ReadToPointerReadsCorrectData()
    {
        // Arrange
        byte[] data = Encoding.ASCII.GetBytes("Test a string that we can read from a stream.");
        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);
        byte[] buffer = new byte[data.Length];
        fixed (byte* pBuffer = buffer)
        {
            GorgonPtr<byte> destBuffer = new(pBuffer, buffer.Length);

            // Act
            reader.ReadToPointer(destBuffer);

            // Assert
            CollectionAssert.AreEqual(data, buffer);
        }
    }

    [TestMethod]
    public unsafe void ReadToPointerThrowsEndOfStreamExceptionWhenPointerIsTooLarge()
    {
        // Arrange
        byte[] data = Encoding.ASCII.GetBytes("Test a string that we can read from a stream.");
        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);
        byte[] buffer = new byte[data.Length + 1]; // Buffer is larger than the stream.
        fixed (byte* pBuffer = buffer)
        {
            GorgonPtr<byte> destBuffer = new(pBuffer, buffer.Length);

            // Act & Assert
            Assert.ThrowsException<EndOfStreamException>(() => reader.ReadToPointer(destBuffer));
        }
    }

    [TestMethod]
    public void ReadToPointerThrowsNullReferenceExceptionWhenPointerIsNull()
    {
        // Arrange
        byte[] data = Encoding.ASCII.GetBytes("Test a string that we can read from a stream.");
        using MemoryStream stream = new(data);
        using BinaryReader reader = new(stream);
        GorgonPtr<byte> nullPointer = GorgonPtr<byte>.NullPtr;

        // Act & Assert
        Assert.ThrowsException<NullReferenceException>(() => reader.ReadToPointer(nullPointer));
    }

    [TestMethod]
    public void ReadValueReadsCorrectData()
    {
        // Arrange
        GorgonPoint data = new(123, 456);
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.Default, true); // leaveOpen is set to true.
        writer.Write(data.X);
        writer.Write(data.Y);
        stream.Position = 0;
        using BinaryReader reader = new(stream);

        // Act
        reader.ReadValue(out GorgonPoint result);
        stream.Position = 0; // Reset the stream position.
        GorgonPoint result2 = reader.ReadValue<GorgonPoint>();

        // Assert
        Assert.AreEqual(data, result);
        Assert.AreEqual(data, result2);
    }

    [TestMethod]
    public void ReadValueThrowsEndOfStreamExceptionWhenStreamIsTooSmall()
    {
        // Arrange
        using MemoryStream stream = new(new byte[1]); // Stream is too small to hold a GorgonPoint.
        using BinaryReader reader = new(stream);

        // Act & Assert
        Assert.ThrowsException<EndOfStreamException>(() => reader.ReadValue<GorgonPoint>(out _));
    }

    [TestMethod]
    public void WriteValueWritesCorrectData()
    {
        // Arrange
        GorgonPoint data = new(123, 456);
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.Default, true); // leaveOpen is set to true.

        // Act
        writer.WriteValue(data);
        writer.WriteValue(in data);

        // Assert
        stream.Position = 0; // Reset the stream position.
        using BinaryReader reader = new(stream);
        GorgonPoint result = new(reader.ReadInt32(), reader.ReadInt32());
        Assert.AreEqual(data, result);

        GorgonPoint result2 = new(reader.ReadInt32(), reader.ReadInt32());
        Assert.AreEqual(data, result2);
    }

    [TestMethod]
    public void WriteRangeWritesCorrectData()
    {
        // Arrange
        GorgonPoint[] data = [new(123, 456), new(789, 012)];
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.Default, true); // leaveOpen is set to true.

        // Act
        writer.WriteRange<GorgonPoint>(data);

        // Assert
        stream.Position = 0; // Reset the stream position.
        using BinaryReader reader = new(stream);
        GorgonPoint result1 = new(reader.ReadInt32(), reader.ReadInt32());
        GorgonPoint result2 = new(reader.ReadInt32(), reader.ReadInt32());
        Assert.AreEqual(data[0], result1);
        Assert.AreEqual(data[1], result2);
    }

    [TestMethod]
    public void ReadRangeReadsCorrectData()
    {
        // Arrange
        GorgonPoint[] data = [new(123, 456), new(789, 012)];
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, Encoding.Default, true); // leaveOpen is set to true.
        foreach (GorgonPoint point in data)
        {
            writer.Write(point.X);
            writer.Write(point.Y);
        }
        stream.Position = 0; // Reset the stream position.
        using BinaryReader reader = new(stream);
        GorgonPoint[] result = new GorgonPoint[2];

        // Act
        reader.ReadRange<GorgonPoint>(result);

        // Assert
        Assert.AreEqual(data[0], result[0]);
        Assert.AreEqual(data[1], result[1]);
    }

    [TestMethod]
    public void ReadRangeThrowsArgumentEmptyExceptionWhenValuesIsEmpty()
    {
        // Arrange
        using MemoryStream stream = new();
        using BinaryReader reader = new(stream);
        GorgonPoint[] result = [];

        // Act & Assert
        Assert.ThrowsException<ArgumentEmptyException>(() => reader.ReadRange<GorgonPoint>(result));
    }

    [TestMethod]
    public void WriteStringWritesCorrectData()
    {
        // Arrange
        string data = "Test string";
        using MemoryStream stream = new();

        // Act
        int bytesWritten = stream.WriteString(data);

        // Assert
        stream.Position = 0; // Reset the stream position.
        using BinaryReader reader = new(stream, Encoding.UTF8);
        int length = reader.Read7BitEncodedInt(); // Read the length prefix.
        string result = new(reader.ReadChars(length));
        Assert.AreEqual(data, result);
        Assert.AreEqual(Encoding.UTF8.GetByteCount(data) + 1, bytesWritten);
    }

    [TestMethod]
    public void WriteStringThrowsIOExceptionWhenStreamIsReadOnly()
    {
        // Arrange
        string data = "Test string";
        using MemoryStream stream = new(new byte[100], false); // Write-only stream.

        // Act & Assert
        Assert.ThrowsException<IOException>(() => stream.WriteString(data));
    }

    [TestMethod]
    public void ReadStringReadsCorrectData()
    {
        // Arrange
        string data = "Test string";
        Encoding encoding = Encoding.UTF8;
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, encoding, true); // leaveOpen is set to true.
        writer.Write(data); // Write the string data.
        stream.Position = 0; // Reset the stream position.

        // Act
        string result = stream.ReadString(encoding);

        // Assert
        Assert.AreEqual(data, result);
    }

    [TestMethod]
    public void ReadStringReadsCorrectDataLargeString()
    {
        // Arrange

        Encoding encoding = Encoding.UTF8;
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, encoding, true); // leaveOpen is set to true.
        writer.Write(LoremIpsum); // Write the string data.
        stream.Position = 0; // Reset the stream position.

        // Act
        string result = stream.ReadString(encoding);

        // Assert
        Assert.AreEqual(LoremIpsum, result);
    }

    [TestMethod]
    public void ReadStringReadsCorrectDataLargeStringUnicode()
    {
        // Arrange

        Encoding encoding = Encoding.Unicode;
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream, encoding, true); // leaveOpen is set to true.
        writer.Write(LoremIpsum); // Write the string data.
        stream.Position = 0; // Reset the stream position.

        // Act
        string result = stream.ReadString(encoding);

        // Assert
        Assert.AreEqual(LoremIpsum, result);
    }

    [TestMethod]
    public void WriteStringWritesCorrectDataLargeString()
    {
        // Arrange

        Encoding encoding = Encoding.UTF8;
        using MemoryStream stream = new();
        stream.WriteString(LoremIpsum, encoding);
        stream.Position = 0;

        using BinaryReader reader = new(stream, encoding); // leaveOpen is set to true.
        string result = reader.ReadString(); // Write the string data.

        // Assert
        Assert.AreEqual(LoremIpsum, result);
    }

    [TestMethod]
    public void WriteStringWritesCorrectDataLargeStringUnicode()
    {
        // Arrange

        Encoding encoding = Encoding.Unicode;
        using MemoryStream stream = new();
        stream.WriteString(LoremIpsum, encoding);
        stream.Position = 0;

        using BinaryReader reader = new(stream, encoding); // leaveOpen is set to true.
        string result = reader.ReadString(); // Write the string data.

        // Assert
        Assert.AreEqual(LoremIpsum, result);
    }

    [TestMethod]
    public void ReadStringThrowsIOExceptionWhenStreamIsAtEnd()
    {
        // Arrange
        using MemoryStream stream = new();

        // Act & Assert
        Assert.ThrowsException<IOException>(() => stream.ReadString());
    }

    [TestMethod]
    public void WriteAndReadStringWithDifferentEncodings()
    {
        // Arrange
        string data = "Test string";
        Encoding encoding1 = Encoding.UTF8;
        Encoding encoding2 = Encoding.ASCII;
        using MemoryStream stream = new();

        // Act
        int bytesWritten1 = stream.WriteString(data, encoding1);
        stream.Position = 0; // Reset the stream position.
        string readData1 = stream.ReadString(encoding1);

        stream.Position = 0; // Reset the stream position.
        int bytesWritten2 = stream.WriteString(data, encoding2);
        stream.Position = 0; // Reset the stream position.
        string readData2 = stream.ReadString(encoding2);

        // Assert
        Assert.AreEqual(data, readData1);
        Assert.AreEqual(encoding1.GetByteCount(data) + 1, bytesWritten1);
        Assert.AreEqual(data, readData2);
        Assert.AreEqual(encoding2.GetByteCount(data) + 1, bytesWritten2);
    }

    [TestMethod]
    public void FormatFileNameEmptyInputReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatFileName();
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatFileNamePathWithoutFilenameReturnsEmptyString()
    {
        string path = "C:\\Some\\Path\\";
        string result = path.FormatFileName();
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatFileNamePathWithIllegalCharactersReturnsSanitizedFilename()
    {
        string path = "C:\\Some\\Path\\file*name.txt";
        string result = path.FormatFileName();
        Assert.AreEqual("file_name.txt", result);
    }

    [TestMethod]
    public void FormatFileNamePathWithLegalCharactersReturnsSameFilename()
    {
        string path = "C:\\Some\\Path\\filename.txt";
        string result = path.FormatFileName();
        Assert.AreEqual("filename.txt", result);
    }

    [TestMethod]
    public void FormatFileNamePathWithAlternateDividerReturnsCorrectFilename()
    {
        string path = "C:/Some/Path/filename.txt";
        string result = path.FormatFileName();
        Assert.AreEqual("filename.txt", result);
    }

    [TestMethod]
    public void FormatDirectoryEmptyInputReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatDirectoryPathWithIllegalCharactersReturnsSanitizedPath()
    {
        string path = @"C:\Some\Invalid*Path\";
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Invalid_Path\", result);
    }

    [TestMethod]
    public void FormatDirectoryPathWithAlternateSeparatorReturnsPathWithStandardSeparator()
    {
        string path = @"C:/Some/Path/";
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\", result);
    }

    [TestMethod]
    public void FormatDirectoryPathWithDoubledSeparatorsReturnsPathWithSingleSeparators()
    {
        string path = @"C:\Some\\Path\";
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\", result);
    }

    [TestMethod]
    public void FormatDirectoryPathWithMultipleConsecutiveSeparatorsInMiddleReturnsPathWithSingleSeparators()
    {
        string path = @"C:\//Some\\\\\Path\/";
        string result = path.FormatDirectory(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\", result);
    }

    [TestMethod]
    public void FormatDirectoryEmptyInputAltSeparatorReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatDirectoryPathWithIllegalCharactersAltSeparatorReturnsSanitizedPath()
    {
        string path = @"C:/Some/Invalid*Path/";
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Invalid_Path/", result);
    }

    [TestMethod]
    public void FormatDirectoryPathWithAlternateSeparatorAltSeparatorReturnsPathWithStandardSeparator()
    {
        string path = @"C:\Some\Path\";
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/", result);
    }

    [TestMethod]
    public void FormatDirectoryPathWithDoubledSeparatorsAltSeparatorReturnsPathWithSingleSeparators()
    {
        string path = @"C:/Some//Path/";
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/", result);
    }

    [TestMethod]
    public void FormatDirectoryPathWithMultipleConsecutiveSeparatorsInMiddleAltSeparatorReturnsPathWithSingleSeparators()
    {
        string path = @"C:/Some/////Path/";
        string result = path.FormatDirectory(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/", result);
    }

    [TestMethod]
    public void FormatPathPartEmptyInputReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatPathPart();
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatPathPartPathWithIllegalCharactersReturnsSanitizedPath()
    {
        string path = "Invalid*Path";
        string result = path.FormatPathPart();
        Assert.AreEqual("Invalid_Path", result);
    }

    [TestMethod]
    public void FormatPathPartPathWithDirectorySeparatorReturnsPathWithUnderscores()
    {
        string path = "Some\\Path";
        string result = path.FormatPathPart();
        Assert.AreEqual("Some_Path", result);
    }

    [TestMethod]
    public void FormatPathPartPathWithAltDirectorySeparatorReturnsPathWithUnderscores()
    {
        string path = "Some/Path";
        string result = path.FormatPathPart();
        Assert.AreEqual("Some_Path", result);
    }

    [TestMethod]
    public void GetPathPartsEmptyInputReturnsEmptyArray()
    {
        string path = string.Empty;
        string[] result = path.GetPathParts(Path.DirectorySeparatorChar);
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void GetPathPartsPathWithDirectorySeparatorReturnsPathParts()
    {
        string path = @"C:\Some\Path";
        string[] result = path.GetPathParts(Path.DirectorySeparatorChar);
        CollectionAssert.AreEqual(_expected, result);
    }

    [TestMethod]
    public void GetPathPartsPathWithAltDirectorySeparatorReturnsPathParts()
    {
        string path = @"C:/Some/Path";
        string[] result = path.GetPathParts(Path.AltDirectorySeparatorChar);
        CollectionAssert.AreEqual(_expected, result);
    }

    [TestMethod]
    public void FormatPathEmptyInputReturnsEmptyString()
    {
        string path = string.Empty;
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void FormatPathWithoutDirectoryName()
    {
        string path = @"file.txt";
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"file.txt", result);
    }

    [TestMethod]
    public void FormatPathWithoutFileName()
    {
        string path = @"c:\some\path\";
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"c:\some\path\", result);
    }

    [TestMethod]
    public void FormatPathPathWithIllegalCharactersReturnsSanitizedPath()
    {
        string path = @"C:\Some\Invalid*Path\file.txt";
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Invalid_Path\file.txt", result);
    }

    [TestMethod]
    public void FormatPathPathWithAltDirectorySeparatorReturnsSanitizedPath()
    {
        string path = @"C:/Some/Invalid*Path/file.txt";
        string result = path.FormatPath(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Invalid_Path/file.txt", result);
    }

    [TestMethod]
    public void FormatPathPathWithIllegalCharactersInFileNameReturnsSanitizedPath()
    {
        string path = @"C:\Some\Path\Invalid*file.txt";
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\Invalid_file.txt", result);
    }

    [TestMethod]
    public void FormatPathPathWithAltDirectorySeparatorAndIllegalCharactersInFileNameReturnsSanitizedPath()
    {
        string path = @"C:/Some/Path/Invalid*file.txt";
        string result = path.FormatPath(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/Invalid_file.txt", result);
    }

    [TestMethod]
    public void FormatPathPathWithDoubledSeparatorsReturnsSanitizedPath()
    {
        string path = @"C:\\Some\\Path\\file.txt";
        string result = path.FormatPath(Path.DirectorySeparatorChar);
        Assert.AreEqual(@"C:\Some\Path\file.txt", result);
    }

    [TestMethod]
    public void FormatPathPathWithAltDoubledSeparatorsReturnsSanitizedPath()
    {
        string path = @"C://Some//Path//file.txt";
        string result = path.FormatPath(Path.AltDirectorySeparatorChar);
        Assert.AreEqual(@"C:/Some/Path/file.txt", result);
    }

    [TestMethod]
    public void ChunkIDEmptyInputThrowsArgumentEmptyException()
    {
        string chunkName = string.Empty;
        Assert.ThrowsException<ArgumentEmptyException>(() => chunkName.ChunkID());
    }

    [TestMethod]
    public void ChunkIDShortInputReturnsCorrectChunkID()
    {
        string chunkName = "Short";
        ulong result = chunkName.ChunkID();
        Assert.AreEqual(0x0000000074726F6853UL, result);
    }

    [TestMethod]
    public void ChunkIDExactLengthInputReturnsCorrectChunkID()
    {
        string chunkName = "ExactLen";
        ulong result = chunkName.ChunkID();
        Assert.AreEqual(0x6E654C7463617845UL, result);
    }

    [TestMethod]
    public void ChunkIDLongInputReturnsCorrectChunkID()
    {
        string chunkName = "TooLongInput";
        ulong result = chunkName.ChunkID();
        Assert.AreEqual(0x49676e6f4c6f6f54UL, result);
    }
}
