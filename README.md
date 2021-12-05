# KrakenProfiler
 An in-game profiler for Unity.
 
---
## Introduction
KrakenProfiler is a lightweight, in-game profiler for games made in unity. It uses the standard Unity profiling tools and builds on top of that. It's highly customizable and open source. You may use it to profile your game dev builds on the native machine or just to show some stats in the final release. I am aware of the fact that you can attach your editor's profiler to a build and profile it there but I find this to be a much easier way of testing my games standalone. I made this in my free time to profile my games on the go. I'd be happy if anyone else found this project useful.

---
## Usage
This piece of software is very easy to use. Simply drag and drop the KrakenProfiler prefab into the first scene of your game (this will usually be the splash screen). Setting it up is easy as well. Almost all the fields in the script have tooltips so you can simply hover over one and know what it exactly does, and for the ones that do not have tooltips, I feel they're pretty self explanatory. For a more detailed description with guiding images refer to the [documentation](https://github.com/himan2104/KrakenProfiler/Documentation.pdf).

### Controls
| Fields | Type | Description |
|--------|------|-------------|
| Show Build Info | Bool/Toggle | Enable or disable the build info that always shows on the screen |
| Product Name Override | String | Override the product name in Build Info. If empty it will use the name in your project settings|
| Show Stats on Boot | Bool/Toggle | Shows the stats on the startup of the application |
| Text Size | Ranged Float/Slider | Size of the text in profiler and build info |
| Text Color | Color | Color of the text. It won't affect the FPS since it always changes its colors based on the value |
| Background Color | Color | Text background color of the profiler |
| UpdateDelay | Ranged Float/Slider | Delay(in seconds) updates of the profiler. Leave 0 to update every frame |
| Average FPS Range | Unsigned Integer | Buffer of Framerate data stored to calculate the average framerate |

### Profiler Recorders
The software provides some common profilers by default. They are:
- Draw Calls
- Batches
- SetPass Calls
- Triangles
- Vertices

Alternatively, you can have your own custom profilers! Simply create a new entry in the Custom Recorders list and provide it with:
- Profiler Category: Select the Category from the drop down.
- Profiler Marker: Input the string identifier of the stat you would like to track.

For more information check the [unity documentation on profiler recorders](https://docs.unity3d.com/2020.2/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html).

### Scripting Reference
There's only one public function accessible to enable or disable the profiler. This will allow you to toggle the profiler on or off on the go. This function maybe used as:

```C#
FindObjectOfType<Himan.KrakenProfiler>().Show(true);    //enable profiler window
FindObjectOfType<Himan.KrakenProfiler>().Show(false);   //disable profiler window
```

---
## Contribution
You can fork this repository and modify it anyway you want to. If you would like to contribute you may share those changes as pull requests. If it's absolutely a necessity for this tool and is written in a clean and tidy way, I'll accept the changes after we have a quick chat and add your name to the contributor's list.