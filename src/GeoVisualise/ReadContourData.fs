module GeoVisualise.ReadContourData

open System

open FSharp.Data

type SpotHeight =
    {
        SpotX: float
        SpotY: float
        Elevation: float
    }

type ContourPoint =
    {
        X: float
        Y: float
    }

type Contour =
    {
        Height: float
        Points: ContourPoint []
    }

type ContoursFile =
    {
        MinX: float
        MaxX: float
        MinY: float
        MaxY: float

        SpotHeights: SpotHeight []
    }

let arrayFloatStringsToTwoTuple arr =
    match arr with
    | [| s1; s2 |] ->
        match Double.TryParse(s1), Double.TryParse(s2) with
        | (true, x1), (true, x2) -> x1, x2
        | _ -> failwithf "Could not parse: %s %s" s1 s2
    | _ -> failwithf "Array did not contain two strings"

let pairFloats (s: string) =
    s.Split(' ')
    |> arrayFloatStringsToTwoTuple

let pairFloatList (s: string) =
    s.Split(' ')
    |> Array.chunkBySize 2
    |> Array.map arrayFloatStringsToTwoTuple

type Contours = XmlProvider< @"data\TL11\TL11.gml">

let readContours (filepath: string) =
    let contours = Contours.Load(filepath)

    let minX, minY = contours.BoundedBy.Envelope.LowerCorner |> pairFloats
    let maxX, maxY = contours.BoundedBy.Envelope.UpperCorner |> pairFloats

    let spotHeights =
        contours.Members
        |> Array.choose (fun m -> m.SpotHeight)
        |> Array.map (fun spotHeight ->
            let x, y = spotHeight.Geometry.Point.Pos |> pairFloats
            let elevation = spotHeight.PropertyValue.Value |> float
            { SpotX = x; SpotY = y; Elevation = elevation })

    let contours =
        contours.Members
        |> Array.choose (fun m -> m.ContourLine)
        |> Array.map (fun contour ->
            let height = contour.PropertyValue.Value |> float
            let points = contour.Geometry.LineString.PosList
                         |> pairFloatList
                         |> Array.map (fun (x, y) -> { X = x; Y = y })
            { Height = height; Points = points })


    {
        MinX = minX
        MaxX = maxX
        MinY = minY
        MaxY = maxY

        SpotHeights = spotHeights
    }