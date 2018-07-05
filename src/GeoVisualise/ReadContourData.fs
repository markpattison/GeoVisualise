module GeoVisualise.ReadContourData

open System

open FSharp.Data

type SpotHeight =
    {
        X: float
        Y: float
        Elevation: float
    }

type ContoursFile =
    {
        MinX: float
        MaxX: float
        MinY: float
        MaxY: float

        SpotHeights: SpotHeight []
    }

let twoFloats (s: string) =
    match s.Split(' ') with
    | [| s1; s2 |] ->
        match Double.TryParse(s1), Double.TryParse(s2) with
        | (true, x1), (true, x2) -> x1, x2
        | _ -> failwithf "Could not parse: %s" s
    | _ -> failwithf "Could not parse: %s" s

type Contours = XmlProvider< @"data\TL11\TL11.gml">

let readContours (filepath: string) =
    let contours = Contours.Load(filepath)

    let minX, minY = contours.BoundedBy.Envelope.LowerCorner |> twoFloats
    let maxX, maxY = contours.BoundedBy.Envelope.UpperCorner |> twoFloats

    let spotHeights =
        contours.Members
        |> Array.choose (fun m -> m.SpotHeight)
        |> Array.map (fun spotHeight ->
            let x, y = spotHeight.Geometry.Point.Pos |> twoFloats
            let elevation = spotHeight.PropertyValue.Value |> float
            { X = x; Y = y; Elevation = elevation })



    {
        MinX = minX
        MaxX = maxX
        MinY = minY
        MaxY = maxY

        SpotHeights = spotHeights
    }