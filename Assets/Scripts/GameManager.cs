using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject plateSpawnPoint;
    [SerializeField] private GameObject platePrefab;
    [SerializeField] private LayerMask plateLayerMask;
    [SerializeField] private LayerMask sortingHolesLayerMask;
    [SerializeField] private List<GameObject> sortingHoles;
    
    [SerializeField] private GameObject playScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject endgameScreen;
    
    [SerializeField] private List<GameObject> liveObjects;

    private static Color[] colors = {Color.green, Color.red, Color.cyan, Color.yellow, Color.magenta, Color.blue};

    public static int score;
    public static int lives;
    public static int collected;

    private List<GameObject> plates = new List<GameObject>();
    
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image collectedImage;
    
    [SerializeField] private List<Light> bgLights;

    void Start()
    {
        ColorSortingHoles();
        TurnOffScreens();
        playScreen.SetActive(true);
    }

    private void ColorSortingHoles()
    {
        sortingHoles[0].GetComponentInChildren<Renderer>().material.color = colors[0];
        sortingHoles[1].GetComponentInChildren<Renderer>().material.color = colors[1];
        sortingHoles[2].GetComponentInChildren<Renderer>().material.color = colors[2];
        sortingHoles[3].GetComponentInChildren<Renderer>().material.color = colors[3];
        sortingHoles[4].GetComponentInChildren<Renderer>().material.color = colors[4];
        sortingHoles[5].GetComponentInChildren<Renderer>().material.color = colors[5];
    }

    private void StartGame()
    {
        ChangeLight();
        StartCoroutine(Game());
    }

    private IEnumerator Game()
    {
        score = 0;
        lives = 3;
        collected = 0;
        collectedImage.fillAmount = 0f;
        scoreText.text = "0";
        while (true)
        {
            if (lives <= 0 || collected >= 10)
            {
                TurnOffScreens();
                endgameScreen.SetActive(true);
                foreach (var p in plates) Destroy(p);
                    StopAllCoroutines();
                break; 
            }

            var plate = Instantiate(platePrefab, plateSpawnPoint.transform.position, 
                Quaternion.Euler(0, 0, 0));
            plates.Add(plate);
            StartCoroutine(PlateLifetime(plate));
            yield return new WaitForSeconds(2f);
        }
    }

    private void ChangeLight()
    {
        switch (Random.Range(0, 3))
        {
            case 0:
                bgLights[0].color = Color.blue;
                bgLights[1].color = Color.red;
                break;
            case 1:
                bgLights[0].color = Color.yellow;
                bgLights[1].color = Color.blue;
                break;
            case 2:
                bgLights[0].color = Color.red;
                bgLights[1].color = Color.blue;
                break;
        }
    }

    private IEnumerator PlateLifetime(GameObject plate)
    {
        var rb = plate.GetComponent<Rigidbody>();
        var bc = plate.GetComponent<BoxCollider>();
        plate.GetComponentInChildren<Renderer>().material.color = colors[Random.Range(0, colors.Length)];
        while (true)
        {

            var platePos = plate.transform.position;

            if (LeanTouch.Fingers.Count > 1  && LeanTouch.Fingers[1].Down
                && Physics.Raycast(Camera.main.ScreenPointToRay(LeanTouch.Fingers[1].ScreenPosition),
                     out var hit, plateLayerMask)
                && hit.collider.gameObject.Equals(bc.gameObject))
            {
                StartCoroutine(PlatePicked(plate));
                yield break;
            }

            if (!Physics.Raycast(platePos, Vector3.down, 2))
            {
                StartCoroutine(DroppedPlate(plate));
                yield break;
            }
            rb.velocity = Vector3.back * 2.6f;
            yield return null;
        }
        
    }

    private IEnumerator PlatePicked(GameObject plate)
    {
        var finger = LeanTouch.Fingers[1];
        var plateTransform = plate.transform;
        var rb = plate.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        while (finger.Down || finger.Set)
        {
            var fingerWorldPos = finger.GetWorldPosition(10, Camera.main);
            var newPos = new Vector3(fingerWorldPos.x, plateTransform.position.y, fingerWorldPos.z);
            plateTransform.position += (newPos - plateTransform.position) / 6 ;
            yield return null;
        }
        rb.isKinematic = false;
        StartCoroutine(DroppedPlate(plate));
    }

    private IEnumerator DroppedPlate(GameObject plate)
    {
        var renderer = plate.GetComponentInChildren<Renderer>();
        var sorted = false;
        while (true)
        {
            if (Physics.BoxCast(plate.transform.position, Vector3.one / 2f,
                -plate.transform.up, out var boxHit, Quaternion.identity, sortingHolesLayerMask)
            && boxHit.collider.gameObject.GetComponentInChildren<Renderer>().material.color.Equals(renderer.material.color))
            {
                score += Random.Range(5, 10);
                scoreText.text = score + "";
                collected++;
                collectedImage.fillAmount = collected / 10f;
                sorted = true;
                print("Collected: " + collected);
                print("Lives: " + lives);
                print("Right Sorted");
            }
            
            var color = renderer.material.color;
            if ( color.a > 0 ) color.a -= 0.05f;
            else
            {
                if (!sorted)
                {
                    lives--;
                    foreach (var liveObject in liveObjects) liveObject.SetActive(false);
                    for (var i = 0; i < lives; i++) liveObjects[i].SetActive(true);
                }
                break;
            }
            renderer.material.color = color;
            
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(plate);
    }

    public void PlayButton()
    {
        TurnOffScreens();
        gameScreen.SetActive(true);
        StartGame();
    }

    private void TurnOffScreens()
    {
        playScreen.SetActive(false);
        gameScreen.SetActive(false);
        endgameScreen.SetActive(false);
    }

















}
