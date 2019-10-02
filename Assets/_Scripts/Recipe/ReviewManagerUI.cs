using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class ReviewManagerUI : MonoBehaviour
{
    public static Recipe thisRecipe;
    public static ReviewManagerUI Instance;
    [SerializeField] private GameObject canvas;

    [Header("Prefabs")]
    [SerializeField] private GameObject labelPrefab;
    [SerializeField] private GameObject infoPrefab;

    [SerializeField] private Transform verticalGroupTrans;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void InitReviewUI(Recipe newRecipe)
    {
        thisRecipe = newRecipe;

        // remove any previous ingredients and directions
        if (verticalGroupTrans.childCount > 1)
        {
            for (int i = 1; i < verticalGroupTrans.childCount; i++)
                Destroy(verticalGroupTrans.GetChild(i).gameObject);
        }

        

        labelPrefab.GetComponentInChildren<Text>().text = "More Reviews";

        for (int i = newRecipe.Reviews.Length - 1; i >= 0; i--)
        {
            Text reviewText = Instantiate(infoPrefab, verticalGroupTrans.transform.position, infoPrefab.transform.rotation,
                verticalGroupTrans).GetComponentInChildren<Text>();

            reviewText.text = newRecipe.Reviews[i];
        }
        
        canvas.SetActive(true);
    }

    public void ShowMoreReviews()
    {
        ReviewManagerUI.Instance.Enable();
    }

    public void Enable()
    {
        this.canvas.SetActive(true);
    }

    public void Disable()
    {
        this.canvas.SetActive(false);
        RecipeManagerUI.Instance.Enable();
    }
}
