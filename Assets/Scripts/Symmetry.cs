using UnityEngine;
using Random = UnityEngine.Random;

public class Symmetry : MonoBehaviour
{ 
    bool horizontalSymmetryResult;
    bool verticalSymmetryResult;
    bool forwardDiagonalSymmetryResult;
    bool backwardDiagonalSymmetryResult;
    
    bool quarterHorizontalSymmetryResult;
    bool quarterVerticalSymmetryResult;
    bool quarterForwardDiagonalSymmetryResult;
    bool quarterBackwardDiagonalSymmetryResult;
    bool lowerIsDominant;

    public void AttemptToApplySymmetry(ref Texture2D texture, int frame, SymmetryConfig configuration) {
        if (frame == 0) {
            ResetAll();
            if (configuration.enforceSomeTypeOfSymmetry && (
                configuration.horizontalSymmetryChance + configuration.verticalSymmetryChance +
                configuration.forwardDiagonalSymmetryChance + configuration.backwardDiagonalSymmetryChance > 0)) {
                while (!horizontalSymmetryResult && !verticalSymmetryResult && 
                       !backwardDiagonalSymmetryResult && !forwardDiagonalSymmetryResult) {
                    DetermineSymmetryDirectionsToApply(configuration);
                }
            }
            else
                DetermineSymmetryDirectionsToApply(configuration);
        }
        if (horizontalSymmetryResult)
            ApplySymmetry(ref texture, SymmetryDirection.Horizontal);
        if (ShouldApplyVerticalSymmetry(configuration.allowMultipleSymmetryTypes))
            ApplySymmetry(ref texture, SymmetryDirection.Vertical);
        if (ShouldApplyForwardDiagonalSymmetry(configuration.allowMultipleSymmetryTypes))
            ApplySymmetry(ref texture, SymmetryDirection.ForwardDiagonal);
        if (ShouldApplyBackwardDiagonalSymmetry(configuration.allowMultipleSymmetryTypes))
            ApplySymmetry(ref texture, SymmetryDirection.BackwardDiagonal);
    }

    void ResetAll() {
        horizontalSymmetryResult =
            verticalSymmetryResult =
                forwardDiagonalSymmetryResult =
                    backwardDiagonalSymmetryResult =
                        quarterHorizontalSymmetryResult =
                            quarterVerticalSymmetryResult =
                                quarterForwardDiagonalSymmetryResult =
                                    quarterBackwardDiagonalSymmetryResult = false;
    }

    void DetermineSymmetryDirectionsToApply(SymmetryConfig configuration) {
        lowerIsDominant = Random.value > .5f;
        
        horizontalSymmetryResult = Random.value < configuration.horizontalSymmetryChance;
        verticalSymmetryResult = Random.value < configuration.verticalSymmetryChance;
        forwardDiagonalSymmetryResult = Random.value < configuration.forwardDiagonalSymmetryChance;
        backwardDiagonalSymmetryResult = Random.value < configuration.backwardDiagonalSymmetryChance;
        
        quarterHorizontalSymmetryResult = horizontalSymmetryResult && Random.value < configuration.quarterHorizontalSymmetryChance;
        quarterVerticalSymmetryResult = verticalSymmetryResult && Random.value < configuration.quarterVerticalSymmetryChance;
        quarterForwardDiagonalSymmetryResult = forwardDiagonalSymmetryResult && Random.value < configuration.quarterForwardDiagonalSymmetryChance;
        quarterBackwardDiagonalSymmetryResult = backwardDiagonalSymmetryResult && Random.value < configuration.quarterBackwardDiagonalSymmetryChance;
    }

    bool ShouldApplyVerticalSymmetry(bool multipleSymmetriesAllowed) {
        if (multipleSymmetriesAllowed || !horizontalSymmetryResult)
            return verticalSymmetryResult;
        return false;
    }

    bool ShouldApplyForwardDiagonalSymmetry(bool multipleSymmetriesAllowed) {
        if (multipleSymmetriesAllowed || (!horizontalSymmetryResult && !verticalSymmetryResult))
            return forwardDiagonalSymmetryResult;
        return false;
    }

    bool ShouldApplyBackwardDiagonalSymmetry(bool multipleSymmetriesAllowed) {
        if (multipleSymmetriesAllowed || 
            (!horizontalSymmetryResult && !verticalSymmetryResult && !forwardDiagonalSymmetryResult)) 
            return backwardDiagonalSymmetryResult;
        return false;
    }

    void ApplySymmetry(ref Texture2D texture, SymmetryDirection direction)
    {
        var halfwayPoint = texture.width / 2;

        for (var rowIndex = 0; rowIndex < texture.height; rowIndex++) {
            for (var columnIndex = 0; columnIndex < texture.width; columnIndex++) {
                int referenceValue;
                switch (direction) {
                    case SymmetryDirection.Horizontal:
                        referenceValue = quarterHorizontalSymmetryResult ? columnIndex : rowIndex;
                        if ((lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                            (!lowerIsDominant && referenceValue <= halfwayPoint + 1))
                            SetSymmetricalPixel(texture, direction, columnIndex, rowIndex, halfwayPoint);
                        break;
                    case SymmetryDirection.Vertical:
                        referenceValue = quarterVerticalSymmetryResult ? rowIndex : columnIndex;
                        if ((lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                            (!lowerIsDominant && referenceValue <= halfwayPoint + 1))
                            SetSymmetricalPixel(texture, direction, columnIndex, rowIndex, halfwayPoint);
                        break;
                    case SymmetryDirection.ForwardDiagonal:
                        if (quarterForwardDiagonalSymmetryResult) {
                            if (lowerIsDominant && columnIndex > rowIndex || !lowerIsDominant && columnIndex < rowIndex)
                                texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(texture.width - rowIndex, texture.width - columnIndex));
                        }
                        else if (!quarterForwardDiagonalSymmetryResult) {
                            if (lowerIsDominant && rowIndex < texture.width - columnIndex || !lowerIsDominant && rowIndex > texture.width - columnIndex)
                                texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(texture.width - rowIndex, texture.width - columnIndex));
                        }
                        break;
                    case SymmetryDirection.BackwardDiagonal:
                        if (quarterBackwardDiagonalSymmetryResult) {
                            if (lowerIsDominant && rowIndex < texture.width - columnIndex || !lowerIsDominant && rowIndex > texture.width - columnIndex)
                                texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(rowIndex, columnIndex));
                        }
                        else 
                            if (lowerIsDominant && columnIndex > rowIndex || !lowerIsDominant && columnIndex < rowIndex)
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