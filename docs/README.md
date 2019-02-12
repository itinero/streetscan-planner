StreetScan
==========

This is the documentation of the street scan tool.

### Overview

This tool:

- Loads a routing network from OSM **for Belgium** (extensions for other countries are possible later)
- Loads a set of locationd from a CSV file.
- Calculates the best path along the locations:
  - Start location is the first location in the CSV.
  - End location can be anywhere.
  - Tries to avoid u-turns where possible.

The result is something like this:

![result](result-wechelderzande.png "Resulting route")

### Manual

This is a cross-platform tools avaible for linux, windows and macos.

#### Setup

Download the latest release here:

[https://github.com/itinero/streetscan-planner/releases/latest](https://github.com/itinero/streetscan-planner/releases/latest)

Make sure you choose the correct download for your platform: 

- Windows: `win10-x65`
- Linux: `linux-x64`
- Macos: `osx-64` 

You get a zip-archive, extract it to an appropriate directory and that's it.

#### Testing

The tool provides a test built-in, run:

- Linux/macos: `.\StreetScan.Planner test`
- Windows: `\StreetScan.Planner.exe test`

If the test is running properly should should see the following:

```
[11:04:00 INF] Building routerdb...
[11:04:00 INF] Downloading Belgium OSM data...
```

This means that the tool hasn't found a local routing network. It will:

- Start downloading OSM data
- Convert it into a routing network.
- Prepare the network for routing by car.

**This can take up to 20mins** but needs to run only once. After the network has been downloaded and built you should see an `output.gpx` file. That's the result of processing the `test.csv` input file.

#### Usage

Running the tool is best done after running the test above to make sure the routing network is properly initialized and things are working as they should. 

The tool can take two types of input files:

- CSV: A comma-separated file with all locations. The first one is taken as the starting point.
- GeoJSON: A GeoJson file with all locations as Point geometries. The first is taken as the starting point.

##### CSV

You can start to create your own input files based on the `test.csv` file. Make sure your CSV is also **comma-seperated and use `.` as a decimal seperator.**

The contents of the file look as follows:

| PKANCODE | COMMUNE_NL | STREET_NL     | HUISNR | LAT           | LON           | 
|----------|------------|---------------|--------|---------------|---------------| 
| 2275     | Lille      | Hoogland      | 85     | 51.2341358125 | 4.83612817117 | 
| 2275     | Lille      | Veenzijde     | 67     | 51.2567636838 | 4.8643172579  | 
| 2275     | Lille      | Gierlebaan    | 4      | 51.2418882157 | 4.82365010831 | 
| 2275     | Lille      | Heikant       | 56     | 51.2296450078 | 4.82798452585 | 
| 2275     | Lille      | Wechelsebaan  | 103    | 51.2497172183 | 4.81676969177 | 
| 2275     | Lille      | Pulsebaan     | 50     | 51.2599269332 | 4.7840406854  | 
| 2275     | Lille      | Haarlebeek    | 5      | 51.2839904513 | 4.83032583221 | 
| 2275     | Lille      | Pimpelmeesweg | 13     | 51.2764403901 | 4.79575669058 | 
| 2275     | Lille      | Heikant       | 74     | 51.2292322544 | 4.82638707063 | 
| 2275     | Lille      | Heggelaan     | 37     | 51.2229807573 | 4.83371775485 | 
| 2275     | Lille      | Eikenlaan     | 37     | 51.2388269374 | 4.83036302955 | 
| 2275     | Lille      | Duinenweg     | 13     | 51.2794449074 | 4.79585938234 | 
| 2275     | Lille      | Moereind      | 34     | 51.2564997812 | 4.79495926373 | 
| 2275     | Lille      | Nieuwstraat   | 34A    | 51.2619512316 | 4.79206247836 | 
| ...      | ...        | ...           | ...    | ...           | ...           | 

The only columns actually used here are `LAT` and `LON`. The tool will take the first line as it's starting location and generate an optimized route along all the locations that come after.

###### GeoJson

This tool also takes GeoJSON files in WGS84 projection. It will read all point geometries from the file and use them as locations. The first point will be taken as the starting point.

###### Running

You can run the tool with custom input as follows:

- Linux/macos: `.\StreetScan.Planner locations.csv`
- Windows: `\StreetScan.Planner.exe locations.csv`

This will generate two output files:

- `locations.csv.gpx` : The resulting GPX file.
- `locations.csv.gpx.geojson` : The same but in GeoJSON (easier to use in GIS tools).
