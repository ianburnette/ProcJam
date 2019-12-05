using UnityEngine;
using Random = UnityEngine.Random;

public class Symmetry : MonoBehaviour {
    SymmetryOutcome additionalFrameCachedSymmetryOutcome;
    
    public void AttemptToApplySymmetry(
        ref Texture2D texture, int frame, SymmetryConfig configuration, bool inheritSymmetry,
        ref SymmetryOutcome symmetryOutcome) {
        if (!inheritSymmetry) {
            if (frame == 0) {
                if (configuration.enforceSomeTypeOfSymmetry && (
                    configuration.horizontalSymmetryChance + configuration.verticalSymmetryChance +
                    configuration.forwardDiagonalSymmetryChance + configuration.backwardDiagonalSymmetryChance > 0)) {
                    while (!symmetryOutcome.horizontalSymmetryResult && !symmetryOutcome.verticalSymmetryResult && 
                           !symmetryOutcome.backwardDiagonalSymmetryResult && !symmetryOutcome.forwardDiagonalSymmetryResult) {
                        symmetryOutcome = DetermineSymmetryDirectionsToApply(configuration);
                    }
                }
                else
                    symmetryOutcome = DetermineSymmetryDirectionsToApply(configuration);

                additionalFrameCachedSymmetryOutcome = symmetryOutcome;
            } else
                symmetryOutcome = additionalFrameCachedSymmetryOutcome;
        }
        
        if (symmetryOutcome.horizontalSymmetryResult)
            ApplySymmetry(ref texture, SymmetryDirection.Horizontal, symmetryOutcome);
        if (ShouldApplyVerticalSymmetry(configuration.allowMultipleSymmetryTypes, symmetryOutcome))
            ApplySymmetry(ref texture, SymmetryDirection.Vertical, symmetryOutcome);
        if (ShouldApplyForwardDiagonalSymmetry(configuration.allowMultipleSymmetryTypes, symmetryOutcome))
            ApplySymmetry(ref texture, SymmetryDirection.ForwardDiagonal, symmetryOutcome);
        if (ShouldApplyBackwardDiagonalSymmetry(configuration.allowMultipleSymmetryTypes, symmetryOutcome))
            ApplySymmetry(ref texture, SymmetryDirection.BackwardDiagonal, symmetryOutcome);
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

    void ApplySymmetry(ref Texture2D texture, SymmetryDirection direction, SymmetryOutcome symmetryOutcome)
    {
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

            // for (var rowIndex = 0; rowIndex < texture.height; rowIndex++) {
           //     for (var columnIndex = 0; columnIndex < texture.width; columnIndex++) {
           //         int referenceValue;
           //         switch (direction) {
           //             case SymmetryDirection.horizontal:
           //                 referenceValue = screwItUpALittle ? columnIndex : rowIndex;
           //                 if ((lowerIsDominant && referenceValue >= halfwayPoint - 1) || (!lowerIsDominant && referenceValue <= halfwayPoint + 1))
           //                     SetSymmetricalPixel(texture, direction, columnIndex, rowIndex, halfwayPoint);
           //                 break;
           //             case SymmetryDirection.vertical:
           //                 referenceValue = screwItUpALittle ? rowIndex : columnIndex;
           //                 if ((lowerIsDominant && referenceValue >= halfwayPoint - 1) || (!lowerIsDominant && referenceValue <= halfwayPoint + 1))
           //                     SetSymmetricalPixel(texture, direction, columnIndex, rowIndex, halfwayPoint);
           //                 break;
           //             case SymmetryDirection.forwardDiagonal:
           //                 lowerIsDominant = true;
           //                 var key = texture.width + 1;
           //                 if ((lowerIsDominant && columnIndex > rowIndex) ||
           //                     (!lowerIsDominant && columnIndex < rowIndex)) {
           //                     var res = new Vector2Int(texture.width - 1 - rowIndex, texture.height - 1 - columnIndex);
           //                     texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(texture.width-1-rowIndex, texture.height-1-columnIndex));
           //                     //IT'S THE ORDER IN WHICH I'M ITERATING THROUGH IT - YOU NEED TO LOOP IN A DIFFERENT DIRECTION
           //                 }
           //             
           //                 if (screwItUpALittle){
           //                     /*twisty screw up
           //                      * if ((lowerIsDominant && columnIndex > rowIndex) || (!lowerIsDominant && columnIndex < rowIndex)) screws it up, twisty-like
           //                      * texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(texture.width + 1 - rowIndex, texture.height + 1 - columnIndex));
           //                      */
           //                     /* nice aesthetically pleasing screw-up
           //                         if ((lowerIsDominant && columnIndex <= rowIndex) || (!lowerIsDominant && rowIndex <= columnIndex))
           //                         texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(texture.width + 1 - rowIndex, texture.height + 1 - columnIndex));
           //                     */
           //               
           //                 }
           //                 else {
           //                     //if ((lowerIsDominant && columnIndex > rowIndex) || (!lowerIsDominant && columnIndex < rowIndex))
           //                     //    texture.SetPixel(columnIndex, rowIndex, );
           //                 }
           //                 break;
           //             case SymmetryDirection.backwardDiagonal:
           //                 if ((lowerIsDominant && columnIndex > rowIndex) || (!lowerIsDominant && columnIndex < rowIndex))
           //                     texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(rowIndex, columnIndex));
           //                 break;
           //         }
           //     }
           // }
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


}

enum SymmetryDirection {
    Horizontal,
    Vertical,
    ForwardDiagonal,
    BackwardDiagonal,
    QuarterVertical,
}