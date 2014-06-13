Observable Master Views
====================

In this app are a number of views.  The scope of this document would be to consolidate a comprehensive
list of views as used by this application in its current state, so that I can re-develop under the hood
what would be required for a new user-interface, including a WPF application-interface.

Piano-layout
----------------------------

### Features

* GDI Rendering of a piano grid, in preparation for requirements

### Requirements/Dependencies

* MIDI editing and parsing API to be prepared.  A midi editor feature
has not yet been supported.

Audio Configuration
----------------------------

The AudioConfigurationView correlates to global audio-buffer settings in `gen.snd.common/Core/TimeConfiguration`.
There are two settings: `SampleRate` and `Latency` which are changed within the UI.

### Features

* Configure buffer settings
* Choose sound-API: DirectSound, Et.

### Requirements/Dependencies

* Requirement One

MIDI Event View
----------------------------

### Features

* Reads out a list of midi events
* I would like to see a DataGrid in WPF and DataGridView in Forms.

Requirements/Dependencies

IMidiView and IMidiViewContainer
----------------------------

`IMidiView` is stored in the core vst library.  [MIDI Event View] 

HELPER DIALOGS
============================

Helper dialogs include any dialog that might be helpful, which is not
entirely dependent upon the main audio-playback process, or is primarily
functional without the application---while of course it is included in the
application.

BMP Calculator
------------------------------

I don't believe this is included in the project

APPLICATION MODELS and/or COMMANDS
===============================

Application commands are mostly visible via the menus of the application or
perhaps are concealed behind a particular view.

Note that any above view, should be partially based on a major API provided
by the application.