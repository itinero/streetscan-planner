# streetscan-planner

[![Release Build](https://github.com/itinero/streetscan-planner/actions/workflows/release.yml/badge.svg)](https://github.com/itinero/streetscan-planner/actions/workflows/release.yml)  

This is a small application to optimize routes to drive along all locations in an area. 

Documentation is available [here](docs/).

## Overview

This tool:

- Loads a routing network from OSM **for Belgium** 
  - Save file with a cutout around the input file locations.
- Loads a set of locations from a CSV file.
- Calculates the best path along the locations:
  - Start location is the first location in the CSV.
  - End location can be anywhere.
  - Tries to avoid u-turns where possible.

The result is something like this:

![result](docs/result-kortemark.png "Resulting route")

