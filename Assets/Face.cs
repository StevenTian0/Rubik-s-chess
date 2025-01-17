
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class Face : MonoBehaviour
{
  public string Name;
  public Color FaceColor;
  public List<string> AdjacentFaces;
  public Vector3 SideNormal;
  public string SideString;
  public Transform CurrentTransform;


  public CubeState cubeState;
  void Start()
  {
    cubeState = FindObjectOfType<CubeState>();
    AssignName();
  }

  void OnDrawGizmos()
  {
    if (cubeState == null) return;

    var stateDict = cubeState.GetStateDictionary();
    var positionInfo = FindFacePosition(Name, stateDict);
    string side = positionInfo.Item1;
    int row = positionInfo.Item2;
    int col = positionInfo.Item3;

    // Draw the X and Y coordinates
    Gizmos.color = Color.white;
    Vector3 position = transform.position;
    string text = $"{row}, {col}";

    // Adjust the size and position of the text as needed
    float size = 0.1f; // Adjust size as needed
    Vector3 offset = new Vector3(0.1f, 0.1f, 0); // Adjust offset as needed

    // Draw the text
    //UnityEditor.Handles.Label(position + offset, text);
  }

  void AssignName()
  {
    if (transform.parent != null)
    {
      // Set the name field to be parent's name + this GameObject's name
      Name = transform.parent.name + "-" + gameObject.name;
    }
    else
    {
      // If there's no parent, just use this GameObject's name
      Name = gameObject.name;
    }
  }

 
  public List<string> FourAdjacentSides()
  {
    var stateDict = cubeState.GetStateDictionary();
    var positionInfo = FindFacePosition(Name, stateDict);
    string side = positionInfo.Item1;
    int row = positionInfo.Item2;
    int col = positionInfo.Item3;
    string[,] matrix = stateDict[side];

    List<string> adjacentFaces = new List<string>();

    if (row == 1 && col == 1) // Center position
    {
      adjacentFaces.AddRange(new string[]
      {
                matrix[0, 1], // Above
                matrix[1, 0], // Left
                matrix[1, 2], // Right
                matrix[2, 1]  // Below
      });
    }
    else if (row == 1 || col == 1) // Edge but not corner
    {
      if (row == 0 && col == 1)
      {
        adjacentFaces.Add(matrix[0, 0]);
        adjacentFaces.Add(matrix[0, 2]);
        adjacentFaces.Add(matrix[1, 1]);
      }
      else if (row == 1 && col == 0)
      {
        adjacentFaces.Add(matrix[0, 0]);
        adjacentFaces.Add(matrix[2, 0]);
        adjacentFaces.Add(matrix[1, 1]);
      }
      else if (row == 1 && col == 2)
      {
        adjacentFaces.Add(matrix[0, 2]);
        adjacentFaces.Add(matrix[2, 2]);
        adjacentFaces.Add(matrix[1, 1]);
      }
      else if (row == 2 && col == 1)
      {
        adjacentFaces.Add(matrix[2, 0]);
        adjacentFaces.Add(matrix[2, 2]);
        adjacentFaces.Add(matrix[1, 1]);
      }
    }
    else // Corner
    {
      if (row == 0 && col == 0)
      {
        adjacentFaces.Add(matrix[0, 1]);
        adjacentFaces.Add(matrix[1, 0]);
      }
      else if (row == 0 && col == 2)
      {
        adjacentFaces.Add(matrix[0, 1]);
        adjacentFaces.Add(matrix[1, 2]);
      }
      else if (row == 2 && col == 0)
      {
        adjacentFaces.Add(matrix[2, 1]);
        adjacentFaces.Add(matrix[1, 0]);
      }
      else if (row == 2 && col == 2)
      {
        adjacentFaces.Add(matrix[2, 1]);
        adjacentFaces.Add(matrix[1, 2]);
      }
      // Add the two adjacent faces on the same side

    }

    // Add the faces from the GameObject's parent's other enabled children
    GameObject currentGameObject = this.gameObject;
    if (currentGameObject != null && currentGameObject.transform.parent.gameObject != null)
    {
      //Debug.Log(currentGameObject.transform.parent);
      foreach (Transform sibling in currentGameObject.transform.parent)
      {
        if (sibling.gameObject != currentGameObject && sibling.gameObject.activeSelf)
        {
          // Assuming the name of the GameObject is the same as the face name in the state dictionary
          string siblingFaceName = sibling.GetComponent<Face>().Name;
          adjacentFaces.Add(siblingFaceName);
        }
      }
    }

    return adjacentFaces;
  }

  public (int finalx, int finaly) CubeTuple((int x, int y) Coordinate)
  {
    if (Coordinate.x == 0)
    {
      //(0,y) goes up or down
      if (Coordinate.y > 0) return (-1, 0);
      else return (1, 0);
    }
    else if (Coordinate.y == 0)
    {
      if (Coordinate.x > 0) return (0, 1);
      else return (0, -1);
    }
    else
    {
      return (0, 0);
    }
  }

  public (String s, int curx, int cury) getSide()
  {

    (String s, int curx, int cury) tuple = FindFacePosition(Name, cubeState.GetStateDictionary());

    return tuple;

  }

  public List<List<(string finalside, int finalx, int finaly)>> GetAllPossiblepositions(string chesstype, int range, (String side, int curx, int cury) current_position)
  {
    string currentSide = current_position.side;
    int currentX = current_position.curx;
    int currentY = current_position.cury;

    List<List<(string finalside, int finalx, int finaly)>> moveinfo = new List<List<(string finalside, int finalx, int finaly)>>();

    if (chesstype == "car")
    {
      List<(int, int)> moves = new List<(int, int)>();
      for (int i = 0; i < range; i++)
      {
        moves.Add((1, 0));
      }
      moveinfo.Add(ResolveUnitStep(current_position, moves));

      List<(int, int)> moves_2 = new List<(int, int)>();
      for (int i = 0; i < range; i++)
      {
        moves_2.Add((0, 1));
      }
      moveinfo.Add(ResolveUnitStep(current_position, moves_2));

      List<(int, int)> moves_3 = new List<(int, int)>();
      for (int i = 0; i < range; i++)
      {
        moves_3.Add((-1, 0));
      }
      moveinfo.Add(ResolveUnitStep(current_position, moves_3));

      List<(int, int)> moves_4 = new List<(int, int)>();

      for (int i = 0; i < range; i++)
      {
        moves_4.Add((0, -1));
      }
      moveinfo.Add(ResolveUnitStep(current_position, moves_4));


    }

        if (chesstype == "bishop")
        {
            // Diagonal moves in the positive X and Y direction
            List<(int, int)> movesXYPositive = new List<(int, int)>();
            for (int i = 0; i < range; i++)
            {
                movesXYPositive.Add((1, 0));
                movesXYPositive.Add((0, 1));
            }
            moveinfo.Add(ResolveUnitStep(current_position, movesXYPositive));

            // Diagonal moves in the positive X and negative Y direction
            List<(int, int)> movesXPositiveYNegative = new List<(int, int)>();
            for (int i = 0; i < range; i++)
            {
                movesXPositiveYNegative.Add((1, 0));
                movesXPositiveYNegative.Add((0, -1));
            }
            moveinfo.Add(ResolveUnitStep(current_position, movesXPositiveYNegative));

            // Diagonal moves in the negative X and positive Y direction
            List<(int, int)> movesXNegativeYPositive = new List<(int, int)>();
            for (int i = 0; i < range; i++)
            {
                movesXNegativeYPositive.Add((-1, 0));
                movesXNegativeYPositive.Add((0, 1));
            }
            moveinfo.Add(ResolveUnitStep(current_position, movesXNegativeYPositive));

            // Diagonal moves in the negative X and Y direction
            List<(int, int)> movesXYNegative = new List<(int, int)>();
            for (int i = 0; i < range; i++)
            {
                movesXYNegative.Add((-1, 0));
                movesXYNegative.Add((0, -1));
            }
            moveinfo.Add(ResolveUnitStep(current_position, movesXYNegative));
        }

        if (chesstype == "horse")
    {
      List<(int, int)> moves = new List<(int, int)>
      {
         (0,1),
         (1, 0),
         (1,0),
      };

      moveinfo.Add(ResolveUnitStep(current_position, moves));

      List<(int, int)> moves_2 = new List<(int, int)>
      {
         (0,-1),
         (1, 0),
         (1,0),
      };

      moveinfo.Add(ResolveUnitStep(current_position, moves_2));

      List<(int, int)> moves_3 = new List<(int, int)>
      {
         (0,1),
         (-1, 0),
         (-1,0),
      };

      moveinfo.Add(ResolveUnitStep(current_position, moves_3));

      List<(int, int)> moves_4 = new List<(int, int)>
      {
         (0,-1),
         (-1, 0),
         (-1,0),
      };

      moveinfo.Add(ResolveUnitStep(current_position, moves_4));

      List<(int, int)> moves_5 = new List<(int, int)>
      {
         (1,0),
         (0, 1),
         (0,1),
      };

      moveinfo.Add(ResolveUnitStep(current_position, moves_5));

      List<(int, int)> moves_6 = new List<(int, int)>
            {
                (-1, 0),
                 (0, 1),
                 (0,1),
            };

      moveinfo.Add(ResolveUnitStep(current_position, moves_6));

      List<(int, int)> moves_7 = new List<(int, int)>
      {
         (1,0),
         (0, -1),
         (0,-1),
      };

      var list = ResolveUnitStep(current_position, moves_7);

      moveinfo.Add(ResolveUnitStep(current_position, moves_7));

      List<(int, int)> moves_8 = new List<(int, int)>
      {
         (-1,0),
         (0, -1),
         (0,-1),
      };

      moveinfo.Add(ResolveUnitStep(current_position, moves_8));
    }
    return moveinfo;
  }


  public List<(string finalside, int finalx, int finaly)> ResolveUnitStep((string initside, int initx, int inity) previousPosition, List<(int moveX, int moveY)> unitMovementVectors)
  {

    List<(string finalSide, int finalX, int finalY)> movementSteps = new List<(string, int, int)>();
    int currentPositionX = previousPosition.initx;
    int currentPositionY = previousPosition.inity;
    var currentSide = previousPosition.initside;
    Debug.Log("INITIAL: " + currentSide);
    for (int i = 0; i < unitMovementVectors.Count; i++)
    {
      Debug.Log(i);
      Debug.Log((currentPositionX, currentPositionY));
      int expectedx = currentPositionX;
      int expectedy = currentPositionY;
      expectedx += unitMovementVectors[i].moveX;
      expectedy += unitMovementVectors[i].moveY;
      Debug.Log("Raw direction vector: " + (unitMovementVectors[i].moveX, unitMovementVectors[i].moveY))
; var direction = GetDirection(unitMovementVectors[i].moveX, unitMovementVectors[i].moveY);
      Debug.Log(direction);
      if (InCurrentSide(expectedx, expectedy, direction))
      {
        var modifiedTuple = CubeTuple((unitMovementVectors[i].moveX, unitMovementVectors[i].moveY));
        movementSteps.Add((currentSide, currentPositionX + modifiedTuple.finalx, currentPositionY + modifiedTuple.finaly));
        currentPositionX += modifiedTuple.finalx;
        currentPositionY += modifiedTuple.finaly;
      }
      else
      {
        var targetSide = GetAdjacentSide(currentSide, direction);
        Debug.Log("Target Side: " + targetSide);
        //map new side index, add to movementSteps, modify current position after add in list movements
        var positionOnNewSide = GetNewSideFirstPosition((currentPositionX, currentPositionY), direction, currentSide);
        movementSteps.Add((targetSide, positionOnNewSide.finalx, positionOnNewSide.finaly));
        currentPositionX = positionOnNewSide.finalx;
        currentPositionY = positionOnNewSide.finaly;
        currentSide = targetSide;
        Debug.Log("Updated current Side: " + currentSide);

        Matrix4x4 transformation = ModifyTuple(previousPosition.initside, targetSide);
        //Debug.Log(transformation);
        for (int j = i + 1; j < unitMovementVectors.Count; j++)
        {
          //Debug.Log(unitMovementVectors[j]);
          Vector4 temp = new Vector4();
          temp.x = unitMovementVectors[j].moveX;
          temp.y = unitMovementVectors[j].moveY;
          //Debug.Log(temp);
          temp = transformation * temp;
          //Debug.Log(temp);
          unitMovementVectors[j] = ((int)temp.x, (int)temp.y);
          //Debug.Log(unitMovementVectors[j]);
        }
      }
    }

    return movementSteps;
  }


  public (string key, int row, int col) FindKeyAndIndex(Dictionary<string, string[,]> dict, string targetValue)
  {
    foreach (var kvp in dict)
    {
      string key = kvp.Key;
      string[,] matrix = kvp.Value;

      for (int i = 0; i < matrix.GetLength(0); i++)
      {
        for (int j = 0; j < matrix.GetLength(1); j++)
        {
          if (matrix[i, j] == targetValue)
          {
            return (key, i, j);
          }
        }
      }
    }
    return (null, -1, -1); // Return this if no match is found
  }

  public List<Face> GetAvailableFaces(string chesstype, int _walkDistance)
  {
    var dict = cubeState.GetStateDictionary();
    var keyAndIndex = FindKeyAndIndex(dict, Name);
    var allPositions = GetAllPossiblepositions(chesstype, _walkDistance, keyAndIndex);
    var faceList = new List<Face>();
    if (chesstype == "horse")
    {
      foreach (var tuplist in allPositions)
      {
        var lastTup = tuplist.Last();
        var faceName = dict[lastTup.finalside][lastTup.finalx, lastTup.finaly];
        string parent = faceName.Split("-")[0];
        string kid = faceName.Split("-")[1];
        GameObject parentname = GameObject.Find(parent);
        Transform child = parentname.transform.Find(kid);
        faceList.Add(child.gameObject.GetComponent<Face>());
      }
    }
    else
    {
      foreach (var tuplist in allPositions)
      {
        foreach (var tup in tuplist)
        {
          var faceName = dict[tup.finalside][tup.finalx, tup.finaly];
          string parent = faceName.Split("-")[0];
          string kid = faceName.Split("-")[1];
          GameObject parentname = GameObject.Find(parent);
          Transform child = parentname.transform.Find(kid);

          Debug.Log(child.gameObject.GetComponent<Face>());
          //child.gameObject.GetComponent<Face>();
          faceList.Add(child.gameObject.GetComponent<Face>());
        }
      }
    }
    return faceList;
  }


  public (int finalx, int finaly) GetNewSideFirstPosition((int currentPositionX, int currentPositionY) currentPosition, string direction, string previousSide)
  {
    switch (direction)
    {
      case "Up":
        switch (currentPosition)
        {
          case (0, 0):
            switch (previousSide)
            {
              case "Left": return (0, 0);
              case "Back": return (0, 2);
              case "Right": return (2, 2);
              case "Up": return (0, 2);
              default: return (2, 0);
            }
          case (0, 1):
            switch (previousSide)
            {
              case "Left": return (1, 0);
              case "Back": return (0, 1);
              case "Right": return (1, 2);
              case "Up": return (0, 1);
              default: return (2, 1);
            }
          case (0, 2):
            switch (previousSide)
            {
              case "Left": return (2, 0);
              case "Back": return (0, 0);
              case "Right": return (0, 2);
              case "Up": return (0, 0);
              default: return (2, 2);
            }
          default: return (-1, -1);
        }
      case "Down":
        switch (currentPosition)
        {
          case (2, 0):
            switch (previousSide)
            {
              case "Left": return (2, 0);
              case "Back": return (2, 2);
              case "Right": return (1, 0);
              case "Down": return (0, 2);
              case "Up": return (0, 0);
              default: return (0, 0);
            }
          case (2, 1):
            switch (previousSide)
            {
              case "Left": return (1, 0);
              case "Back": return (2, 1);
              case "Right": return (1, 0);
              case "Down": return (1, 2);
              case "Up": return (0, 1);

              default: return (0, 1);
            }
          case (2, 2):
            switch (previousSide)
            {
              case "Left": return (0, 0);
              case "Back": return (2, 0);
              case "Right": return (1, 0);
              case "Down": return (2, 2);
              default: return (0, 2);
            }
          default: return (-1, -1);
        }
      case "Left":
        switch (currentPosition)
        {
          case (0, 0):
            switch (previousSide)
            {
              case "Up": return (0, 0);
              case "Down": return (2, 2);
              default: return (0, 2);
            }
          case (1, 0):
            switch (previousSide)
            {
              case "Up": return (0, 1);
              case "Down": return (2, 1);
              default: return (1, 2);
            }
          case (2, 0):
            switch (previousSide)
            {
              case "Up": return (0, 2);
              case "Down": return (2, 0);
              default: return (2, 2);
            }
          default: return (-1, -1);
        }
      case "Right":
        switch (currentPosition)
        {
          case (0, 2):
            switch (previousSide)
            {
              case "Up": return (0, 2);
              case "Down": return (2, 2);
              default: return (0, 0);
            }
          case (1, 2):
            switch (previousSide)
            {
              case "Up": return (0, 1);
              case "Down": return (1, 2);
              default: return (1, 0);
            }
          case (2, 2):
            switch (previousSide)
            {
              case "Up": return (0, 0);
              case "Down": return (0, 2);
              default: return (2, 0);
            }
          default: return (-1, -1);
        }

      default:
        return (-1, -1);
    }
  }

  public Matrix4x4 MultiplyMatrixByScalar(Matrix4x4 matrix, float scalar)
  {
    Matrix4x4 result = new Matrix4x4();

    result.m00 = matrix.m00 * scalar;
    result.m01 = matrix.m01 * scalar;
    result.m02 = matrix.m02 * scalar;
    result.m03 = matrix.m03 * scalar;

    result.m10 = matrix.m10 * scalar;
    result.m11 = matrix.m11 * scalar;
    result.m12 = matrix.m12 * scalar;
    result.m13 = matrix.m13 * scalar;

    result.m20 = matrix.m20 * scalar;
    result.m21 = matrix.m21 * scalar;
    result.m22 = matrix.m22 * scalar;
    result.m23 = matrix.m23 * scalar;

    result.m30 = matrix.m30 * scalar;
    result.m31 = matrix.m31 * scalar;
    result.m32 = matrix.m32 * scalar;
    result.m33 = matrix.m33 * scalar;

    return result;
  }
  public Matrix4x4 ModifyTuple(string initSide, string targetSide)
  {
    Matrix4x4 counterClockwise = Create2DRotationMatrix(90.0f);
    Matrix4x4 clockwise = Create2DRotationMatrix(-90.0f);
    Matrix4x4 flip = Create2DRotationMatrix(180.0f);

    switch (initSide)
    {
      case "Up":
        switch (targetSide)
        {
          case "Left": return counterClockwise;
          case "Up": return flip;
          case "Right": return clockwise;
          case "Down": return Matrix4x4.identity;
        }
        break;
      case "Down":
        switch (targetSide)
        {
          case "Up": return Matrix4x4.identity;
          case "Down": return flip;
          case "Right": return counterClockwise;
          case "Left": return clockwise;
        }
        break;
      case "Left":
        switch (targetSide)
        {
          case "Up": return clockwise;
          case "Down": return counterClockwise;
          default: return Matrix4x4.identity;
        }
      case "Right":
        switch (targetSide)
        {
          case "Up": return counterClockwise;
          case "Down": return clockwise;
          default: return Matrix4x4.identity;
        }
      case "Front":
        return Matrix4x4.identity;
      case "Back":
        switch (targetSide)
        {
          case "Up":
          case "Down": return flip;
          default: return Matrix4x4.identity;
        }
    }

    // Default return in case none of the cases match
    return Matrix4x4.identity;
  }

  // Helper method to create a 2D rotation matrix given an angle in degrees
  private Matrix4x4 Create2DRotationMatrix(float angleDegrees)
  {
    Matrix4x4 matrix = Matrix4x4.identity;
    float theta = angleDegrees * Mathf.Deg2Rad; // Convert degrees to radians

    matrix.m00 = Mathf.Cos(theta);
    matrix.m01 = -Mathf.Sin(theta);
    matrix.m10 = Mathf.Sin(theta);
    matrix.m11 = Mathf.Cos(theta);

    return matrix;
  }

  public string GetDirection(int x, int y)
  {
    Debug.Log("Getting Direction");
    if (x != 0) // The vector is horizontal
    {
      return x > 0 ? "Right" : "Left";
    }
    else if (y != 0) // The vector is vertical
    {
      return y > 0 ? "Up" : "Down";
    }
    else // The vector is (0, 0)
    {
      return "None"; // Or throw an exception, based on how you want to handle this case
    }
  }


  public bool InCurrentSide(int x, int y, string direction)
  {
    if ((x == 0) && direction == "Up")
    {
      return false;
    }
    else if ((y == 0) && direction == "Left")
    {
      return false;
    }
    else if ((x == 2) && direction == "Down")
    {
      return false;
    }
    else if ((y == 2) && direction == "Right")
    {
      return false;
    }
    else
    {
      return true;
    }
  }

  public string GetAdjacentSide(string currentSide, string direction)
  {
    Debug.Log("PASSING IN DIRECTION" + direction);
    switch (currentSide)
    {
      case "Front":
        switch (direction)
        {
          case "Up": return "Up";
          case "Down": return "Down";
          case "Left": return "Left";
          case "Right": return "Right";
          default: return "Invalid direction";
        }

      case "Back":
        switch (direction)
        {
          case "Up": return "Up";
          case "Down": return "Down";
          case "Left": return "Right";
          case "Right": return "Left";
          default: return "Invalid direction";
        }

      case "Up":
        switch (direction)
        {
          case "Up": return "Back";
          case "Down":
            {
              Debug.Log("WE ARE HERE");
              return "Front";
            }

          case "Left": return "Left";
          case "Right": return "Right";
          default: return "Invalid direction";
        }

      case "Down":
        switch (direction)
        {

          case "Up":
            {
              Debug.Log("WE ARE HERE");
              return "Front";
            }
          case "Down": return "Back";
          case "Left": return "Left";
          case "Right": return "Right";
          default: return "Invalid direction";
        }

      case "Left":
        switch (direction)
        {
          case "Up": return "Up";
          case "Down": return "Down";
          case "Left": return "Back";
          case "Right": return "Front";
          default: return "Invalid direction";
        }

      case "Right":
        switch (direction)
        {
          case "Up": return "Up";
          case "Down": return "Down";
          case "Left": return "Front";
          case "Right": return "Back";
          default: return "Invalid direction";
        }

      default:
        return "Invalid side";
    }
  }


  public (string, int, int) FindFacePosition(string faceName, Dictionary<string, string[,]> cubeState)
  {
    foreach (var side in cubeState)
    {
      string sideName = side.Key;
      string[,] matrix = side.Value;

      for (int i = 0; i < matrix.GetLength(0); i++) // Rows
      {
        for (int j = 0; j < matrix.GetLength(1); j++) // Columns
        {
          if (matrix[i, j] == faceName)
          {
            return (sideName, i, j);
          }
        }
      }
    }

    return ("Not Found", -1, -1); // Return this if the face name is not found
  }
  public BasicPiece getCurrentPiece()
  {
    BasicPiece p = transform.GetChild(0).gameObject.GetComponent<BasicPiece>();

    return p;
  }
  public GameObject FindfaceByname(string faceName)
  {
    GameObject face = GameObject.Find(faceName);
    return face;
  }

}
