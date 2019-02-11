StreetScan
==========

This is the documentation on the street scan tool.

### Overview

This tool:

- Loads a routing network from OSM **for Belgium** (extensions for other countries are possible later)
- Loads a set of locationd from a CSV file.
- Calculates the best path along the locations:
  - Start location is the first location in the CSV.
  - End location can be anywhere.
  - Tries to avoid u-turns where possible.

The result is something like this:

![result](result.png "Resulting route")

### Usage

This is a cross-platform tools avaible for linux, windows and macos.

#### Setup

Download the latest release here:

[https://github.com/itinero/streetscan-planner/releases](https://github.com/itinero/streetscan-planner/releases)

Make sure you choose the correct download for your platform:

- Windows: 
- Linux:
- Macos: 

You get a zip-archive, extract it to an appropriate directory and that's it.

#### Testing

The tool provides a test