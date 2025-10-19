using UnityEngine;

public enum StudentState
{
    Idle,
    WalkingOnSight,
    Seated,
    SeatedAndEating
}

[RequireComponent(typeof(Transform))]
public class StudentController : MonoBehaviour
{
    [Header("Eating Settings")]
    public bool hasFood = false;
    public float eatingDuration = 3f;

    [HideInInspector]
    public StudentState state = StudentState.Idle;

    [HideInInspector]
    public bool ignoreNextInput = false; // prevent instant toggle after SitPrompt

    private float eatingTimer = 0f;

    void Update()
    {
        if (state == StudentState.SeatedAndEating)
        {
            eatingTimer -= Time.deltaTime;
            if (eatingTimer <= 0f)
                FinishEating();
        }
    }

    public void PickUpFood()
    {
        hasFood = true;
        if (state == StudentState.Seated)
            StartEating();
    }

    public void StartEating()
    {
        if (!hasFood) return;
        state = StudentState.SeatedAndEating;
        eatingTimer = eatingDuration;
    }

    public void FinishEating()
    {
        hasFood = false;
        state = StudentState.Seated;
    }

    public void SitDown()
    {
        if (state != StudentState.SeatedAndEating)
            state = StudentState.Seated;
    }

    public void StandUp()
    {
        state = StudentState.Idle;
    }

    public void SetWalkingOnSight()
    {
        if (state != StudentState.Seated && state != StudentState.SeatedAndEating)
            state = StudentState.WalkingOnSight;
    }

    public void SetIdle()
    {
        if (state != StudentState.Seated && state != StudentState.SeatedAndEating)
            state = StudentState.Idle;
    }
}
