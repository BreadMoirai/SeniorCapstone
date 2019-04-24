﻿using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages Directions building for PublishingManagerUI
///
/// Ruben Sanchez
/// </summary>
public class DirectionBuilderView : MonoBehaviour
{
    [SerializeField] private GameObject addButton;

    private InputField inputField;

    private string direction;

    private void Awake()
    {
        inputField = GetComponentInChildren<InputField>();
    }

    public void SetDirection(string newDirection)
    {
        direction = newDirection;

        // if direction has been filled, auto add a new builder
        if(!string.IsNullOrEmpty(direction))
            AddNewDirection();
    }

    public string GetDirection()
    {
        return inputField.text;
    }

    public void AddNewDirection()
    {
        // return if either field has not been populated
        if (string.IsNullOrEmpty(inputField.text))
            return;

        // add another instance of this builder to the list
        PublishingManagerUI.Instance.AddDirectiontBuilder();

        // disabled add button to expose delete button
        addButton.gameObject.SetActive(false);
    }

    public void RemoveDirection()
    {
        PublishingManagerUI.Instance.RemoveBuilder(gameObject);
    }
}