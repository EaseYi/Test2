using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class GameManager : Singleton<GameManager>
{
    public CharacterStats playerStats;

    private CinemachineVirtualCamera virtualCamera;

    List<IEndGameObsever> endGameObsevers = new List<IEndGameObsever>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    public void RigisiterPlayer(CharacterStats player)
    {
        playerStats = player;

        virtualCamera =FindAnyObjectByType<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.Follow = playerStats.transform;
            virtualCamera.LookAt= playerStats.transform;
        }
    }
    public void AddObserver(IEndGameObsever obsever)
    { 
        endGameObsevers.Add(obsever);
    }
    public void RemoveObserver(IEndGameObsever obsever)
    { 
        endGameObsevers.Remove(obsever);
    }
    public void NotifyObservers() 
    {
        foreach (var observer in endGameObsevers)
        { 
            observer.EndNotify();
        }
    }
    public Transform GetEntrance()
    {
        foreach (var item in FindObjectsOfType<TransitionDestination>())
        {
            if (item.destinationTag == TransitionDestination.DestinationTag.Enter)
                return item.transform;
        }
        return null;
    }
}
