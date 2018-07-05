module GeoVisualise.CreateSpotHeightVertices

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open ReadContourData

let width = 5.0f
let height = 5.0f

let createVerticesForSpotHeight spotHeight =
    let v1 = VertexPositionNormalTexture(Vector3(single spotHeight.X, single spotHeight.Y + width, single spotHeight.Elevation), Vector3(), Vector2())
    let v2 = VertexPositionNormalTexture(Vector3(single spotHeight.X + 0.866f * width, single spotHeight.Y - 0.5f * width, single spotHeight.Elevation), Vector3(), Vector2())
    let v3 = VertexPositionNormalTexture(Vector3(single spotHeight.X - 0.866f * width, single spotHeight.Y - 0.5f * width, single spotHeight.Elevation), Vector3(), Vector2())
    let vTop = VertexPositionNormalTexture(Vector3(single spotHeight.X, single spotHeight.Y, single spotHeight.Elevation + height), Vector3(), Vector2())

    [| v1; v2; vTop; v2; v3; vTop; v3; v1; vTop |]

let createVertices spotHeights =
    spotHeights
    |> Array.collect createVerticesForSpotHeight