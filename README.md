# Illusion.ObjectMap

I like the Koikatsu Sunshine maps a lot, but sometimes I wanna customize them to my needs for specific scenes or events. This mod was made with the intent of allowing you to edit maps, within reason.

This plugin allows you to:
- Move individual objects
- Enable/Disable individual objects
- Manipulate map lights (Highly recommend getting [LightSettings](https://github.com/starstormhun/StarPlugins))
- Use MaterialEditor on map objects
- Reset objects back to their original position (look on the left toolbar for the Reset Transform button).

What it doesn't let you do:
- Delete map objects
- Reparent map objects
- Parent stuff TO map objects
- Copy map objects
- Make her come back... :(

Known Issues:
- Not really an issue, depending on the time of day map selection some objects will not show up. This is normal and they are there just invisible until you change the time of day. This is done to ensure the feature works properly.
- Maps with many objects will slow your FPS to a crawl. To avoid this, in version [1.1.4](https://github.com/krypto5863/Illusion.ObjectMap/releases/tag/1.1.4), a configurable object limit was added that ignores maps that have more objects than the limit. To help with this, you can use [Performancer](https://github.com/starstormhun/StarPlugins) to help with the load. However, this will not solve the problem.

[CharaStudio_dtXGFUaYcZ.webm](https://github.com/user-attachments/assets/3a32f57c-ada7-429e-bd91-b7debb4bc988)


### Install
Make sure you have the latest [IllusionModdingAPI](https://github.com/IllusionMods/IllusionModdingAPI)
Then, just drop the DLL into bepinex/plugins
