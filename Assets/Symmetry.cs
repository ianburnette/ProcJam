using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Symmetry : MonoBehaviour
{ 
    [SerializeField, Range(0f,1f)] float horizontalSymmetryChance;
    [SerializeField, Range(0f,1f)] float verticalSymmetryChance;
    [SerializeField, Range(0f,1f)] float forwardDiagonalSymmetryChance;
    [SerializeField, Range(0f,1f)] float backwardDiagonalSymmetryChance;

    [SerializeField] bool randomizeSymmetryDominantSideEachFrame;

    bool horizontalSymmetryResult;
    bool verticalSymmetryResult;
    bool forwardDiagonalSymmetryResult;
    bool backwardDiagonalSymmetryResult;
    bool lowerIsDominant;

    [SerializeField] bool screwItUpALittle;

    Texture2D localTexture;
    
    public void AttemptToApplySymmetry(ref Texture2D texture, int frame) {
        localTexture = texture;
        if (frame == 0) {
            horizontalSymmetryResult = Random.value < horizontalSymmetryChance;
            verticalSymmetryResult = Random.value < verticalSymmetryChance;
            forwardDiagonalSymmetryResult = Random.value < forwardDiagonalSymmetryChance;
            backwardDiagonalSymmetryResult = Random.value < backwardDiagonalSymmetryChance;
            lowerIsDominant = Random.value > .5f;
        }
        if (horizontalSymmetryResult) StartCoroutine(ApplySymmetry(SymmetryDirection.horizontal));
        if (verticalSymmetryResult) StartCoroutine(ApplySymmetry(SymmetryDirection.vertical));
        if (forwardDiagonalSymmetryResult) StartCoroutine(ApplySymmetry(SymmetryDirection.forwardDiagonal));
        if (backwardDiagonalSymmetryResult) StartCoroutine(ApplySymmetry(SymmetryDirection.backwardDiagonal));
    }

    IEnumerator ApplySymmetry(SymmetryDirection direction)
    {
        var halfwayPoint = localTexture.width / 2;
        if (randomizeSymmetryDominantSideEachFrame)
            lowerIsDominant = Random.value > .5f;

        if (direction == SymmetryDirection.horizontal ||
            direction == SymmetryDirection.vertical ||
            direction == SymmetryDirection.backwardDiagonal) {
            for (var rowIndex = 0; rowIndex < localTexture.height; rowIndex++) {
                for (var columnIndex = 0; columnIndex < localTexture.width; columnIndex++) {
                    int referenceValue;
                    switch (direction) {
                        case SymmetryDirection.horizontal:
                            referenceValue = screwItUpALittle ? columnIndex : rowIndex;
                            if ((lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                                (!lowerIsDominant && referenceValue <= halfwayPoint + 1))
                                SetSymmetricalPixel(localTexture, direction, columnIndex, rowIndex, halfwayPoint);
                            yield return new WaitForEndOfFrame();
                            localTexture.Apply();

                            break;
                        case SymmetryDirection.vertical:
                            referenceValue = screwItUpALittle ? rowIndex : columnIndex;
                            if ((lowerIsDominant && referenceValue >= halfwayPoint - 1) ||
                                (!lowerIsDominant && referenceValue <= halfwayPoint + 1))
                                SetSymmetricalPixel(localTexture, direction, columnIndex, rowIndex, halfwayPoint);
                            yield return new WaitForEndOfFrame();
                            localTexture.Apply();

                            break;
                        case SymmetryDirection.backwardDiagonal:
                            if ((lowerIsDominant && columnIndex > rowIndex) ||
                                (!lowerIsDominant && columnIndex < rowIndex))
                                localTexture.SetPixel(columnIndex, rowIndex, localTexture.GetPixel(rowIndex, columnIndex));
                            yield return new WaitForEndOfFrame();
                            localTexture.Apply();
                            break;
                    }
                }
            }
        }
        else
        {
            for (var rowIndex = localTexture.height; rowIndex > 0; rowIndex--) {
                for (var columnIndex = 0; columnIndex < localTexture.width; columnIndex++) {
                    if ((lowerIsDominant && columnIndex > rowIndex) || (!lowerIsDominant && columnIndex < rowIndex)) {
                        localTexture.SetPixel(columnIndex, rowIndex,
                            localTexture.GetPixel(localTexture.width - rowIndex, localTexture.width - columnIndex));
                        yield return new WaitForEndOfFrame();
                        localTexture.Apply();

                    }
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
           yield return null;
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