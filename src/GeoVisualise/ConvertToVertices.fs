module GeoVisualise.ConvertToVertices

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open ReadElevationData

let convert asc =
    let nCols = asc.NumCols
    let nRows = asc.NumRows
    let cellSize = single asc.CellSize
    let xBase = single asc.XllCorner
    let yBase = single asc.YllCorner + cellSize * single nRows

    let vertices = Array.init (nCols * nRows) (fun i ->
        let x, y = i % nCols, nCols - 1 - i / nCols
        let xf, yf = single x, single y

        VertexPosition(new Vector3(xBase + xf * cellSize, yBase + yf * cellSize, single asc.Data.[x, y]))
        )
    vertices

let indices nCols nRows =
    let indices =
        seq {
            for x in [0 .. nCols - 2] do
                for y in [0 .. nRows - 2] do
                    yield x     + nCols * y
                    yield x     + nCols * (y + 1)
                    yield x + 1 + nCols * y
                    yield x + 1 + nCols * y
                    yield x + nCols * (y + 1)
                    yield x + 1 + nCols * (y + 1)
        } |> Seq.toArray
    indices