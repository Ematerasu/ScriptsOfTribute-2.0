using UnityEngine;
using ScriptsOfTribute;
using TMPro;
using System.Collections.Generic;
using ScriptsOfTribute.Serializers;

public class PatronManager : MonoBehaviour
{
    [SerializeField] private GameObject patronPrefab;
    [SerializeField] private Transform[] patronSlots = new Transform[5];
    [SerializeField] private SoTGameManager soTGameManager;

    private Dictionary<PatronId, GameObject> _spawnedPatrons = new();

    public void InitializePatrons(PatronId[] patronIds)
    {
        for (int i = 0; i < patronIds.Length; i++)
        {
            PatronId patronId = patronIds[i];
            Transform slot = patronSlots[i];
            GameObject obj = Instantiate(patronPrefab, slot);
            obj.name = patronId.ToString();
            obj.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            sr.sprite = LoadPatronSprite(patronId);

            PatronClickable clickable = obj.GetComponent<PatronClickable>();
            clickable.Initialize(patronId);

            _spawnedPatrons[patronId] = obj;
            BoardManager.Instance.RegisterPatronTransform(patronId, slot);
        }
    }

    public PlayerEnum GetFavorable(PatronId patronId)
    {
        return soTGameManager.PatronStates.GetFor(patronId);
    }

    public void UpdateObjects(PatronStates newStates)
    {
        foreach(var kvp in newStates.All)
        {
            var script = _spawnedPatrons[kvp.Key].GetComponent<PatronClickable>();
            script.favoring = kvp.Value;
            script.UpdatePatronCircleRotation();
        }
    }

    private Sprite LoadPatronSprite(PatronId patronId)
    {
        string path = $"Sprites/Patrons/{patronId}";
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogError($"Nie znaleziono sprite'a patrona pod ścieżką: {path}");
        }
        return sprite;
    }

}
