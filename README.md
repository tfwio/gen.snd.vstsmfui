gen.snd.vstsmfui
================

VST2.0 host + midi parser test — using NAudio &amp; VSTNET

See: [gen.snd] for compilation detail;  This is the development repository containing a solution (`*.sln`) file loading all `*.csproj` files as well as the dependencies.

OBJECTIVE/STATUS
----------------

### INITIAL GOAL

* **Initial goal** of this project to see if building a working VST24-HOST in DOTNET was possible provided a few requisites to what I consider “working”.
* **Requisite #1**: Use and perfect the software-only MIDI parser.  (this means that no "MIDI" device-driver is used here).
* **Requisite #2**: Testing is/was done using a sYXG or YAMAHA XG VST instrument to be used in addition to one VST effect in the audio chain to play a single (multitrack) midi file.
* **Testing**: The MIDI file I predominantly used for is a rendition of Robert Miles "Children".

### CURRENT STATUS

Many of the above requisites have been lived up to with some glitches and exceptions.

### THE FUTURE

I would like to see a programable effect processor come into working status using MIDI constructs to do the programming.

BUG TRACKING
----

Tacking bugs and issues [here][issues].

Soon enough, documentation will evolve---explaining where prescedence of bug-fixing is going to start.

[gen.snd.vstsmfui]: https://github.com/tfwio/gen.snd.vstsmfui
[gen.snd.vst]: https://github.com/tfwio/gen.snd.vst
[gen.snd]: https://github.com/tfwio/gen.snd
[gen.snd.smf]: (https://github.com/tfwio/gen.snd.smf
[issues]: https://github.com/tfwio/gen.snd/issues