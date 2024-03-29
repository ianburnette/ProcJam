﻿using System;
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
        var halfX = texture.width / 2;
        var halfY = texture.height / 2;
        var halfZ = texture.depth / 2;
        var lowDominant = true;// symmetryOutcome.lowerIsDominant;
        
        for (var x = 0; x < texture.width; x++) {
            for (var z = 0; z < texture.depth; z++) {
                for (var y = 0; y < texture.height; y++) {
                    var setSymmetrical = false;
                    switch (direction) {
                        case SymmetryDirection3D.EastTopToWestBottom:
                            if (CompareTo(x, texture.height - y - 1, lowDominant)) setSymmetrical = true; break;
                        case SymmetryDirection3D.SouthBottomToNorthTop:
                            if (CompareTo(z, x, lowDominant)) setSymmetrical = true; break;
                        case SymmetryDirection3D.WestBottomToEastTop:
                            if (CompareTo(x, y, lowDominant)) setSymmetrical = true; break;
                        case SymmetryDirection3D.SouthTopToNorthBottom:
                            if (CompareTo(z, texture.width - x - 1, lowDominant)) setSymmetrical = true; break;
                        case SymmetryDirection3D.SouthEastCenterToNorthWestCenter:
                            if (CompareTo(z, texture.height - y - 1, lowDominant)) setSymmetrical = true; break;
                        case SymmetryDirection3D.SouthWestCenterToNorthEastCenter:
                            if (CompareTo(z, y, lowDominant)) setSymmetrical = true; break;
                        case SymmetryDirection3D.SouthCenterToNorthCenterVertical:
                            if (CompareTo(y, halfY - 1, lowDominant)) setSymmetrical = true; break;
                        case SymmetryDirection3D.SouthCenterToNorthCenterHorizontal:
                            if (CompareTo(x, halfX - 1, lowDominant)) setSymmetrical = true; break;
                        case SymmetryDirection3D.CenterUpToCenterDown:
                            if (CompareTo(z, halfZ - 1, lowDominant)) setSymmetrical = true; break;
                    }

                    if (setSymmetrical) 
                        SetSymmetricalVoxel(texture, direction, y, x, z, halfX, halfY, halfZ);
                }
            }
        }
    }

    bool CompareTo(int input, int comparison, bool lowDominant) => 
        (lowDominant && input > comparison) || 
        (!lowDominant && input < comparison);

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
        Texture3D texture, SymmetryDirection3D direction, int x, int y, int z, int halfX, int halfY, int halfZ) {
        Vector3Int readCoordinates;
        switch (direction) {
            case SymmetryDirection3D.EastTopToWestBottom:
                readCoordinates = new Vector3Int(texture.width - y - 1, texture.height - x - 1, z);
                break;
            case SymmetryDirection3D.SouthBottomToNorthTop:
                readCoordinates = new Vector3Int(x, z, y);
                break;
            case SymmetryDirection3D.WestBottomToEastTop:
                readCoordinates = new Vector3Int(y, x, z);
                break;
            case SymmetryDirection3D.SouthTopToNorthBottom:
                readCoordinates = new Vector3Int(x, texture.height - z - 1, texture.depth - y - 1);
                break;
            case SymmetryDirection3D.SouthEastCenterToNorthWestCenter:
                readCoordinates = new Vector3Int(texture.width - z - 1, y, texture.depth - x - 1);
                break;
            case SymmetryDirection3D.SouthWestCenterToNorthEastCenter:
                readCoordinates = new Vector3Int(z, y, x);
                break;
            case SymmetryDirection3D.SouthCenterToNorthCenterVertical:
                readCoordinates = new Vector3Int(halfY - (x - halfY) - 1, y, z);
                break;
            case SymmetryDirection3D.SouthCenterToNorthCenterHorizontal:
                readCoordinates = new Vector3Int(x, halfY - (y + 1 - halfY), z);
                break;
            case SymmetryDirection3D.CenterUpToCenterDown:
                readCoordinates = new Vector3Int(x, y, halfZ - (z + 1 - halfZ));
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

// https://images.slideplayer.com/26/8501691/slides/slide_24.jpg
public enum SymmetryDirection3D {
    EastTopToWestBottom,
    SouthBottomToNorthTop,
    WestBottomToEastTop,
    SouthTopToNorthBottom,
    SouthEastCenterToNorthWestCenter,
    SouthWestCenterToNorthEastCenter,
    SouthCenterToNorthCenterVertical,
    SouthCenterToNorthCenterHorizontal,
    CenterUpToCenterDown,
}