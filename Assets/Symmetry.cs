using System.Diagnostics.Contracts;
using UnityEngine;
using Random = UnityEngine.Random;

public class Symmetry : MonoBehaviour
{ 
    bool horizontalSymmetryResult;
    bool verticalSymmetryResult;
    bool forwardDiagonalSymmetryResult;
    bool backwardDiagonalSymmetryResult;
    bool lowerIsDominant;

    public void AttemptToApplySymmetry(ref Texture2D texture, int frame, Configuration configuration) {
        if (frame == 0) {
            horizontalSymmetryResult = Random.value < configuration.horizontalSymmetryChance;
            verticalSymmetryResult = Random.value < configuration.verticalSymmetryChance;
            forwardDiagonalSymmetryResult = Random.value < configuration.forwardDiagonalSymmetryChance;
            backwardDiagonalSymmetryResult = Random.value < configuration.backwardDiagonalSymmetryChance;
            lowerIsDominant = Random.value > .5f;
        }
        if (horizontalSymmetryResult) ApplySymmetry(ref texture, SymmetryDirection.horizontal, configuration);
        if (verticalSymmetryResult) ApplySymmetry(ref texture, SymmetryDirection.vertical, configuration);
        if (forwardDiagonalSymmetryResult) ApplySymmetry(ref texture, SymmetryDirection.forwardDiagonal, configuration);
        if (backwardDiagonalSymmetryResult) ApplySymmetry(ref texture, SymmetryDirection.backwardDiagonal, configuration);
    }

    void ApplySymmetry(ref Texture2D texture, SymmetryDirection direction, Configuration configuration)
    {
        var halfwayPoint = texture.width / 2;

        for (var rowIndex = 0; rowIndex < texture.height; rowIndex++) {
            for (var columnIndex = 0; columnIndex < texture.width; columnIndex++) {
                int referenceValue;
                switch (direction) {
                    case SymmetryDirection.horizontal:
                        referenceValue = configuration.allowQuarterSymmetry ? columnIndex : rowIndex;
                        if ((lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                            (!lowerIsDominant && referenceValue <= halfwayPoint + 1))
                            SetSymmetricalPixel(texture, direction, columnIndex, rowIndex, halfwayPoint);
                        break;
                    case SymmetryDirection.vertical:
                        referenceValue = configuration.allowQuarterSymmetry ? rowIndex : columnIndex;
                        if ((lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                            (!lowerIsDominant && referenceValue <= halfwayPoint + 1))
                            SetSymmetricalPixel(texture, direction, columnIndex, rowIndex, halfwayPoint);
                        break;
                    case SymmetryDirection.forwardDiagonal:
                        if (configuration.allowQuarterSymmetry) {
                            if (lowerIsDominant && columnIndex > rowIndex || !lowerIsDominant && columnIndex < rowIndex)
                                texture.SetPixel(columnIndex, rowIndex,
                                    texture.GetPixel(texture.width - rowIndex, texture.width - columnIndex));
                        }
                        else if (!configuration.allowQuarterSymmetry) {
                            if (lowerIsDominant && rowIndex < texture.width - columnIndex ||
                                !lowerIsDominant && rowIndex > texture.width - columnIndex)
                                texture.SetPixel(columnIndex, rowIndex,
                                    texture.GetPixel(texture.width - rowIndex, texture.width - columnIndex));
                        }
                        break;
                    case SymmetryDirection.backwardDiagonal:  
                        if (!configuration.allowQuarterSymmetry) {
                            if ((lowerIsDominant && columnIndex > rowIndex) || (!lowerIsDominant && columnIndex < rowIndex))
                                texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(rowIndex, columnIndex));
                        }
                        else if (configuration.allowQuarterSymmetry) {
                            if (lowerIsDominant && rowIndex < texture.width - columnIndex || !lowerIsDominant && rowIndex > texture.width - columnIndex)
                                texture.SetPixel(columnIndex, rowIndex, texture.GetPixel(texture.width - rowIndex, texture.width - columnIndex));
                        }
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
            case SymmetryDirection.horizontal:
                readCoordinates = new Vector2Int(x, halfwayPoint - (y + 1 - halfwayPoint));
                break;
            case SymmetryDirection.vertical:
                readCoordinates = new Vector2Int(halfwayPoint - (x + 1 - halfwayPoint), y);
                break;
            case SymmetryDirection.forwardDiagonal:
                break;
            case SymmetryDirection.backwardDiagonal:
                break;
        }
        texture.SetPixel(x, y, texture.GetPixel(readCoordinates.x, readCoordinates.y));
    }

}

enum SymmetryDirection {
    horizontal,
    vertical,
    forwardDiagonal,
    backwardDiagonal
}