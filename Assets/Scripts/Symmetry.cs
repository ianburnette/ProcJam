using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Symmetry : MonoBehaviour {
    SymmetryOutcome additionalFrameCachedSymmetryOutcome;

    public void AttemptToApplySymmetry(ref GeneratedVoxelModel voxelModel, SymmetryConfig3D configuration,
        ref SymmetryOutcome3D symmetryOutcome) {

        symmetryOutcome = Determine3DSymmetryDirectionsToApply(configuration);

        foreach (var direction in symmetryOutcome.symmetryDirections) 
            ApplySymmetry(ref voxelModel.modelData, direction, symmetryOutcome);
    }
    
    public void AttemptToApplySymmetry(
        ref Texture2D texture, int frame, SymmetryConfig configuration, bool inheritSymmetry,
        ref SymmetryOutcome symmetryOutcome) {
        if (inheritSymmetry)
            DetermineSymmetry(frame == 0, configuration, ref symmetryOutcome);

        if (symmetryOutcome.horizontalSymmetryResult)
            ApplySymmetry(ref texture, SymmetryDirection.Horizontal, symmetryOutcome);
        if (ShouldApplyVerticalSymmetry(configuration.allowMultipleSymmetryTypes, symmetryOutcome))
            ApplySymmetry(ref texture, SymmetryDirection.Vertical, symmetryOutcome);
        if (ShouldApplyForwardDiagonalSymmetry(configuration.allowMultipleSymmetryTypes, symmetryOutcome))
            ApplySymmetry(ref texture, SymmetryDirection.ForwardDiagonal, symmetryOutcome);
        if (ShouldApplyBackwardDiagonalSymmetry(configuration.allowMultipleSymmetryTypes, symmetryOutcome))
            ApplySymmetry(ref texture, SymmetryDirection.BackwardDiagonal, symmetryOutcome);
    }

    void DetermineSymmetry(
        bool master, SymmetryConfig configuration, ref SymmetryOutcome symmetryOutcome) {
        if (master) {
            if (configuration.enforceSomeTypeOfSymmetry && (
                configuration.horizontalSymmetryChance + configuration.verticalSymmetryChance +
                configuration.forwardDiagonalSymmetryChance + configuration.backwardDiagonalSymmetryChance > 0)) {
                while (!symmetryOutcome.horizontalSymmetryResult && !symmetryOutcome.verticalSymmetryResult &&
                       !symmetryOutcome.backwardDiagonalSymmetryResult &&
                       !symmetryOutcome.forwardDiagonalSymmetryResult) {
                    symmetryOutcome = DetermineSymmetryDirectionsToApply(configuration);
                }
            } else
                symmetryOutcome = DetermineSymmetryDirectionsToApply(configuration);

            additionalFrameCachedSymmetryOutcome = symmetryOutcome;
        } else
            symmetryOutcome = additionalFrameCachedSymmetryOutcome;
    }

    SymmetryOutcome DetermineSymmetryDirectionsToApply(SymmetryConfig configuration) {
        var lowerIsDominant = Random.value > .5f;
        var horizontalSymmetryResult = Random.value < configuration.horizontalSymmetryChance;
        var verticalSymmetryResult = Random.value < configuration.verticalSymmetryChance;
        var forwardDiagonalSymmetryResult = Random.value < configuration.forwardDiagonalSymmetryChance;
        var backwardDiagonalSymmetryResult = Random.value < configuration.backwardDiagonalSymmetryChance;
        var quarterHorizontalSymmetryResult = horizontalSymmetryResult && Random.value < configuration.quarterHorizontalSymmetryChance;
        var quarterVerticalSymmetryResult = verticalSymmetryResult && Random.value < configuration.quarterVerticalSymmetryChance;
        var quarterForwardDiagonalSymmetryResult = forwardDiagonalSymmetryResult && Random.value < configuration.quarterForwardDiagonalSymmetryChance;
        var quarterBackwardDiagonalSymmetryResult = backwardDiagonalSymmetryResult && Random.value < configuration.quarterBackwardDiagonalSymmetryChance;

        return new SymmetryOutcome(horizontalSymmetryResult, verticalSymmetryResult, forwardDiagonalSymmetryResult,
            backwardDiagonalSymmetryResult,
            quarterHorizontalSymmetryResult, quarterVerticalSymmetryResult, quarterForwardDiagonalSymmetryResult,
            quarterBackwardDiagonalSymmetryResult, lowerIsDominant);
    }

    SymmetryOutcome3D Determine3DSymmetryDirectionsToApply(SymmetryConfig3D configuration) {
        var outcome = new SymmetryOutcome3D();
        
        foreach (SymmetryDirection3D direction in Enum.GetValues(typeof(SymmetryDirection3D))) {
            if (Random.value < configuration.ChanceOf(direction)) {
                if (outcome.symmetryDirections.Count == 0 ||
                    configuration.allowMultipleSymmetryTypes && outcome.symmetryDirections.Count > 0) {
                    outcome.symmetryDirections.Add(direction);
                }
            }
        }

        return outcome;
    }

    bool ShouldApplyVerticalSymmetry(bool multipleSymmetriesAllowed, SymmetryOutcome symmetryOutcome) {
        if (multipleSymmetriesAllowed || !symmetryOutcome.horizontalSymmetryResult)
            return symmetryOutcome.verticalSymmetryResult;
        return false;
    }

    bool ShouldApplyForwardDiagonalSymmetry(bool multipleSymmetriesAllowed, SymmetryOutcome symmetryOutcome) {
        if (multipleSymmetriesAllowed || (!symmetryOutcome.horizontalSymmetryResult && 
                                          !symmetryOutcome.verticalSymmetryResult))
            return symmetryOutcome.forwardDiagonalSymmetryResult;
        return false;
    }

    bool ShouldApplyBackwardDiagonalSymmetry(bool multipleSymmetriesAllowed, SymmetryOutcome symmetryOutcome) {
        if (multipleSymmetriesAllowed || 
            (!symmetryOutcome.horizontalSymmetryResult && 
             !symmetryOutcome.verticalSymmetryResult && 
             !symmetryOutcome.forwardDiagonalSymmetryResult)) 
            return symmetryOutcome.backwardDiagonalSymmetryResult;
        return false;
    }

    void ApplySymmetry(ref Texture2D texture, SymmetryDirection direction, SymmetryOutcome symmetryOutcome) {
        var halfwayPoint = texture.width / 2;

        for (var rowIndex = 0; rowIndex < texture.height; rowIndex++) {
            for (var columnIndex = 0; columnIndex < texture.width; columnIndex++) {
                int referenceValue;
                switch (direction) {
                    case SymmetryDirection.Horizontal:
                        referenceValue = symmetryOutcome.quarterHorizontalSymmetryResult ? columnIndex : rowIndex;
                        if ((symmetryOutcome.lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                            (!symmetryOutcome.lowerIsDominant && referenceValue <= halfwayPoint + 1))
                            SetSymmetricalPixel(texture, direction, columnIndex, rowIndex, halfwayPoint);
                        break;
                    case SymmetryDirection.Vertical:
                        referenceValue = symmetryOutcome.quarterVerticalSymmetryResult ? rowIndex : columnIndex;
                        if ((symmetryOutcome.lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                            (!symmetryOutcome.lowerIsDominant && referenceValue <= halfwayPoint + 1))
                            SetSymmetricalPixel(texture, direction, columnIndex, rowIndex, halfwayPoint);
                        break;
                    case SymmetryDirection.ForwardDiagonal:
                        if (symmetryOutcome.quarterForwardDiagonalSymmetryResult) {
                            if (symmetryOutcome.lowerIsDominant && columnIndex > rowIndex || !symmetryOutcome.lowerIsDominant && columnIndex < rowIndex)
                                texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(texture.width - rowIndex, texture.width - columnIndex));
                        }
                        else {
                            if (symmetryOutcome.lowerIsDominant && rowIndex < texture.width - columnIndex || !symmetryOutcome.lowerIsDominant && rowIndex > texture.width - columnIndex)
                                texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(texture.width - rowIndex, texture.width - columnIndex));
                        }
                        break;
                    case SymmetryDirection.BackwardDiagonal:
                        if (symmetryOutcome.quarterBackwardDiagonalSymmetryResult) {
                            if (symmetryOutcome.lowerIsDominant && rowIndex < texture.width - columnIndex || !symmetryOutcome.lowerIsDominant && rowIndex > texture.width - columnIndex)
                                texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(rowIndex, columnIndex));
                        }
                        else 
                            if (symmetryOutcome.lowerIsDominant && columnIndex > rowIndex || !symmetryOutcome.lowerIsDominant && columnIndex < rowIndex)
                                texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(rowIndex, columnIndex));
                        break;
                }
            }
        }
    }

    void ApplySymmetry(ref Texture3D texture, SymmetryDirection3D direction, SymmetryOutcome3D symmetryOutcome) {
        var halfwayPoint = texture.width / 2;
        var lowDominant = symmetryOutcome.lowerIsDominant;
        
        for (var x = 0; x < texture.height; x++) {
            for (var z = 0; z < texture.depth; z++) {
                for (var y = 0; y < texture.width; y++) {
                    switch (direction) {
                        //case SymmetryDirection.Horizontal:
                        //    referenceValue = symmetryOutcome.quarterHorizontalSymmetryResult ? columnIndex : rowIndex;
                        //    if ((symmetryOutcome.lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                        //        (!symmetryOutcome.lowerIsDominant && referenceValue <= halfwayPoint + 1))
                        //        SetSymmetricalVoxel(texture, direction, columnIndex, rowIndex, depthIndex, halfwayPoint);
                        //    break;
                        //case SymmetryDirection.Vertical:
                        //    referenceValue = symmetryOutcome.quarterVerticalSymmetryResult ? rowIndex : columnIndex;
                        //    if ((symmetryOutcome.lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                        //        (!symmetryOutcome.lowerIsDominant && referenceValue <= halfwayPoint + 1))
                        //        SetSymmetricalVoxel(texture, direction, columnIndex, rowIndex, depthIndex, halfwayPoint);
                        //    break;
                        //case SymmetryDirection.ForwardDiagonal:
                        //    if (symmetryOutcome.quarterForwardDiagonalSymmetryResult) {
                        //        if (symmetryOutcome.lowerIsDominant && columnIndex > rowIndex ||
                        //            !symmetryOutcome.lowerIsDominant && columnIndex < rowIndex)
                        //            texture.SetPixel(columnIndex, rowIndex, depthIndex,
                        //                texture.GetPixel(
                        //                    texture.width - rowIndex, 
                        //                    texture.width - columnIndex, 
                        //                    texture.depth - depthIndex));
                        //    } else {
                        //        if (symmetryOutcome.lowerIsDominant && rowIndex < texture.width - columnIndex ||
                        //            !symmetryOutcome.lowerIsDominant && rowIndex > texture.width - columnIndex)
                        //            texture.SetPixel(columnIndex, rowIndex, depthIndex,
                        //                texture.GetPixel(
                        //                    texture.width - rowIndex, 
                        //                    texture.width - columnIndex, 
                        //                    texture.depth - depthIndex));
                        //    }
                        //
                        //    break;
                        //case SymmetryDirection.BackwardDiagonal:
                        //    if (symmetryOutcome.quarterBackwardDiagonalSymmetryResult) {
                        //        if (symmetryOutcome.lowerIsDominant && rowIndex < texture.width - columnIndex ||
                        //            !symmetryOutcome.lowerIsDominant && rowIndex > texture.width - columnIndex)
                        //            texture.SetPixel(columnIndex, rowIndex, depthIndex, 
                        //                texture.GetPixel(rowIndex, columnIndex, depthIndex));
                        //    } else if (symmetryOutcome.lowerIsDominant && columnIndex > rowIndex ||
                        //               !symmetryOutcome.lowerIsDominant && columnIndex < rowIndex)
                        //        texture.SetPixel(columnIndex, rowIndex, depthIndex, 
                        //            texture.GetPixel(rowIndex, columnIndex, depthIndex));
//
                        //    break;
                        case SymmetryDirection3D.TopLeftToBottomRight:
                            if (x<y)//(lowDominant && x < y || !lowDominant && x > y)
                                SetSymmetricalVoxel(texture, direction, y, x, z, halfwayPoint);
                            break;
                        case SymmetryDirection3D.FrontBottomToTopBack:
                            break;
                        case SymmetryDirection3D.BottomLeftToTopRight:
                            if (lowDominant && x > y || !lowDominant && x < y)
                                SetSymmetricalVoxel(texture, direction, y, x, z, halfwayPoint);
                            break;
                        case SymmetryDirection3D.FrontTopToBottomBack:
                            break;
                        case SymmetryDirection3D.FrontRightToBackLeft:
                            break;
                        case SymmetryDirection3D.FrontLeftToBackRight:
                            break;
                        case SymmetryDirection3D.FrontCenterToBackCenterVertical:
                            if (lowDominant && y > halfwayPoint || !lowDominant && y < halfwayPoint)
                                SetSymmetricalVoxel(texture, direction, y, x, z, halfwayPoint);
                            break;
                        case SymmetryDirection3D.FrontCenterToBackCenterHorizontal:
                            if (lowDominant && x > halfwayPoint || !lowDominant && x < halfwayPoint)
                                SetSymmetricalVoxel(texture, direction, y, x, z, halfwayPoint);
                            break;
                        case SymmetryDirection3D.MiddleTopToMiddleBottomVertical:
                            if (lowDominant && z > halfwayPoint || !lowDominant && z < halfwayPoint)
                                SetSymmetricalVoxel(texture, direction, y, x, z, halfwayPoint);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                }
            }
        }
    }

    static void SetSymmetricalPixel(Texture2D texture, SymmetryDirection direction, int x, int y, int halfwayPoint) {
        var readCoordinates = new Vector2Int();
        switch (direction) {
            case SymmetryDirection.Horizontal:
                readCoordinates = new Vector2Int(x, halfwayPoint - (y + 1 - halfwayPoint));
                break;
            case SymmetryDirection.Vertical:
                readCoordinates = new Vector2Int(halfwayPoint - (x + 1 - halfwayPoint), y);
                break;
            case SymmetryDirection.ForwardDiagonal:
                break;
            case SymmetryDirection.BackwardDiagonal:
                break;
        }
        texture.SetPixel(x, y, texture.GetPixel(readCoordinates.x, readCoordinates.y));
    }

    static void SetSymmetricalVoxel(
        Texture3D texture, SymmetryDirection3D direction, int x, int y, int z, int halfwayPoint) {
        var readCoordinates = new Vector3Int();
        switch (direction) {
            case SymmetryDirection3D.TopLeftToBottomRight:
                readCoordinates = new Vector3Int(texture.height - y + 1,texture.width - x + 1, z);
                break;
            case SymmetryDirection3D.FrontBottomToTopBack:
                break;
            case SymmetryDirection3D.BottomLeftToTopRight:
                readCoordinates = new Vector3Int(y, x, z);
                break;
            case SymmetryDirection3D.FrontTopToBottomBack:
                break;
            case SymmetryDirection3D.FrontRightToBackLeft:
                break;
            case SymmetryDirection3D.FrontLeftToBackRight:
                break;
            case SymmetryDirection3D.FrontCenterToBackCenterVertical:
                readCoordinates = new Vector3Int(halfwayPoint - (x + 1 - halfwayPoint), y, z);
                break;
            case SymmetryDirection3D.FrontCenterToBackCenterHorizontal:
                readCoordinates = new Vector3Int(x, halfwayPoint - (y + 1 - halfwayPoint), z);
                break;
            case SymmetryDirection3D.MiddleTopToMiddleBottomVertical:
                readCoordinates = new Vector3Int(x, y, halfwayPoint - (z + 1 - halfwayPoint));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        texture.SetPixel(x, y, z, texture.GetPixel(readCoordinates.x, readCoordinates.y, readCoordinates.z));
    }
}

enum SymmetryDirection {
    Horizontal,
    Vertical,
    ForwardDiagonal,
    BackwardDiagonal,
    QuarterVertical,
}

public enum SymmetryDirection3D {
    TopLeftToBottomRight,
    FrontBottomToTopBack,
    BottomLeftToTopRight,
    FrontTopToBottomBack,
    FrontRightToBackLeft,
    FrontLeftToBackRight,
    FrontCenterToBackCenterVertical,
    FrontCenterToBackCenterHorizontal,
    MiddleTopToMiddleBottomVertical,
}