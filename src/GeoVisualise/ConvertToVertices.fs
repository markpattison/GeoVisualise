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

    let minX = xBase
    let maxX = xBase + cellSize * single (nCols - 1)
    let minY = zBase
    let maxY = zBase + cellSize * single (nRows - 1)

    let vertices = Array.init (nCols * nRows) (fun i ->
        let x, z = i % nCols, nCols - 1 - i / nCols
        let xf, zf = single x, single z

        VertexPositionNormalTexture(Vector3(xBase + xf * cellSize, single asc.Data.[x, z], zBase + zf * cellSize), Vector3(), Vector2()))

    for x in [0 .. nCols - 2] do
        for z in [0 .. nRows - 2] do
            let indexBottomLeft  = x     + nCols * z
            let indexTopLeft     = x     + nCols * (z + 1)
            let indexBottomRight = x + 1 + nCols * z
            let indexTopRight    = x + 1 + nCols * (z + 1)

            let bottomLeft = Vector3(0.0f, single asc.Data.[x, z], 0.0f)
            let bottomRight = Vector3(cellSize, single asc.Data.[x + 1, z], 0.0f)
            let topLeft = Vector3(0.0f, single asc.Data.[x, z + 1], cellSize)
            let topRight = Vector3(cellSize, single asc.Data.[x + 1, z + 1], cellSize)

            let triangle1Normal = Vector3.Cross(bottomLeft - topLeft, bottomLeft - bottomRight) |> Vector3.Normalize

            vertices.[indexBottomLeft].Normal <- vertices.[indexBottomLeft].Normal + triangle1Normal
            vertices.[indexTopLeft].Normal <- vertices.[indexTopLeft].Normal + triangle1Normal
            vertices.[indexBottomRight].Normal <- vertices.[indexBottomRight].Normal + triangle1Normal

            let triangle2Normal = Vector3.Cross(topLeft - topRight, topLeft - bottomRight) |> Vector3.Normalize

            vertices.[indexBottomRight].Normal <- vertices.[indexBottomRight].Normal + triangle2Normal
            vertices.[indexTopLeft].Normal <- vertices.[indexTopLeft].Normal + triangle2Normal
            vertices.[indexTopRight].Normal <- vertices.[indexTopRight].Normal + triangle2Normal
    
    vertices |> Array.iter (fun v -> v.Normal.Normalize())

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