module GeoVisualise.ReadElevationData

open System.IO
open FSharp.Data
open FSharp.Data.CsvExtensions

type AscFile =
    {
        NumCols: int
        NumRows: int
        XllCorner: float
        YllCorner: float
        CellSize: float
        NoDataValue: float option
        Data: float [,]
    }

let (|OptionalNamedValue|_|) name (s: string) =
    match s.Split(' ') with
    | [| n; v |] when n = name -> Some v
    | _ -> None

let (|NamedValue|) name (s: string) =
    match s with
    | OptionalNamedValue name v -> v
    | _ -> failwithf "Failed reading %s" name

let readAsc (stream: StreamReader) =
    let nCols = match stream.ReadLine() with | NamedValue "ncols" x -> x.AsInteger()
    let nRows = match stream.ReadLine() with | NamedValue "nrows" x -> x.AsInteger()
    let xllCorner = match stream.ReadLine() with | NamedValue "xllcorner" x -> x.AsFloat()
    let yllCorner = match stream.ReadLine() with | NamedValue "yllcorner" x -> x.AsFloat()
    let cellSize = match stream.ReadLine() with | NamedValue "cellsize" x -> x.AsFloat()
    let noDataValue, nextLine =
        match stream.ReadLine() with
        | OptionalNamedValue "nodata_value" x -> x.AsFloat() |> Some, None
        | line -> None, Some line
        
    let dataLines =
        seq {
            match nextLine with | Some line -> yield line | None -> ()
            while true do yield stream.ReadLine()
        }

    let data : float [,] = Array2D.zeroCreate nCols nRows

    dataLines
    |> Seq.take nRows
    |> Seq.iteri (fun row line ->
        let values = line.Split(' ')
        for col in 0 .. (nCols - 1) do
            data.[row, col] <- values.[col].AsFloat())

    {
        NumCols = nCols
        NumRows = nRows
        XllCorner = xllCorner
        YllCorner = yllCorner
        CellSize = cellSize
        NoDataValue = noDataValue
        Data = data
    }
