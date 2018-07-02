module GeoVisualise.ConvertToVertices

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open ReadElevationData

let convert asc =
    let nCols = asc.NumCols
    let nRows = asc.NumRows
    let cellSize = single asc.CellSize
    let xBase = single asc.XllCorner
    let zBase = single asc.YllCorner + cellSize * single nRows

    let vertices = Array.init (nCols * nRows) (fun i ->
        let x, z = i % nCols, nCols - 1 - i / nCols
        let xf, zf = single x, single z

        VertexPosition(new Vector3(xBase + xf * cellSize, single asc.Data.[x, z], zBase + zf * cellSize))
        )
    
    let minX = xBase
    let maxX = xBase + cellSize * single (nCols - 1)
    let minY = zBase
    let maxY = zBase + cellSize * single (nRows - 1)

    vertices, minX, maxX, minY, maxY

let indices nCols nRows =
    let indices =
        seq {
            for x in [0 .. nCols - 2] do
                for z in [0 .. nRows - 2] do
                    yield x     + nCols * z
                    yield x     + nCols * (z + 1)
                    yield x + 1 + nCols * z
                    yield x + 1 + nCols * z
                    yield x + nCols * (z + 1)
                    yield x + 1 + nCols * (z + 1)
        } |> Seq.toArray
    indices