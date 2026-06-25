using Dreamteck.Splines;
using Evaluation;
using System.Collections.Generic;
using UnityEngine;

public class RoadObstacle : MonoBehaviour
{
    [HideInInspector] public Circuit circuit;
    [SerializeField] private int m_obstacleSpawnRate = 25;
    [SerializeField] private float m_obstacleDistance = 40f;
    [SerializeField] private int m_offsetMaxSize = 5;
    [SerializeField] private GameObject m_obstaclePrefabs;

    private List<GameObject> m_obstacles = new List<GameObject>();
    private Transform m_carSpawnPoint;

    public void SetObstacle()
    {
        m_carSpawnPoint = circuit.GetCarSpawnPoint();

        for (int i = 0; i < circuit.GetRoads().Count; i++)
        {
            SplineComputer spline = circuit.GetRoads()[i].GetSpline();
            double carSpawnPointPercent = i == 0 ? spline.Project(m_carSpawnPoint.position).percent : 0f;
            float splineLenght = spline.CalculateLength();

            for (float spawnDist = m_obstacleDistance; spawnDist < splineLenght; spawnDist += m_obstacleDistance)
            {
                if (m_obstacleSpawnRate >= Random.Range(0, 101))
                {
                    double percent = spawnDist / splineLenght + carSpawnPointPercent;
                    SplineSample splineSample = spline.Evaluate(percent > 1 ? 1 : percent);
                    Vector3 position = splineSample.position;
                    int offsetSize = Random.Range(0, m_offsetMaxSize + 1);
                    int offsetDirection = Random.Range(0, 2);

                    if (offsetDirection == 0)
                        position += splineSample.right * offsetSize;
                    else
                        position -= splineSample.right * offsetSize;

                    m_obstacles.Add(Instantiate(m_obstaclePrefabs, position, Quaternion.Euler(0, Random.Range(0, 360), 0), transform));
                }
            }
        }

        EvaluationTests.SetMaxObstacle(m_obstacles.Count);
    }

    public void DeleteObstacle()
    {
        for (int i = 0; i < m_obstacles.Count; i++)
            Destroy(m_obstacles[i]);
        m_obstacles.Clear();
    }
}