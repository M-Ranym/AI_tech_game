using System.Collections;
using UnityEngine;

public class RoboPathMover : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 8f;

    [Header("Jump Up")]
    public float jumpArcHeight = 1.8f;
    public float jumpArcDuration = 0.5f;
    public float jumpPrepX = 0.3f;
    public float jumpPrepY = -0.1f;
    public float jumpPrepDuration = 0.12f;

    [Header("Jump Down")]
    public float jumpDownArcDuration = 0.45f;
    public float jumpDownPrepX = 0.3f;
    public float jumpDownPrepY = 0.15f;
    public float jumpDownPrepDuration = 0.12f;

    [Header("Rock (duck and hold)")]
    public float rockDuckY = -0.4f;  // how far down to duck
    public float rockDuckDuration = 0.15f;  // time to duck
    public float rockHoldDuration = 0.5f;   // time held low
    public float rockRiseDuration = 0.15f;  // time to rise back

    [Header("Zip (fast slide forward)")]
    public float zipSlideX = 1.5f;   // how far forward
    public float zipSlideY = 0.3f;   // slight upward arc
    public float zipDuration = 0.35f;  // total zip time

    [Header("Action Duration")]
    public float actionDuration = 0.5f;

    [Header("Shake")]
    public float shakeDuration = 0.6f;
    public float shakeMagnitude = 0.18f;
    public float shakeFrequency = 25f;

    public IEnumerator MoveToSlot(Transform target)
    {
        while (Vector2.Distance(transform.position, target.position) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, target.position, walkSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target.position;
    }

    public IEnumerator PlayAction(RoboPathCore.RobotAction action)
    {
        switch (action)
        {
            case RoboPathCore.RobotAction.Jump:
                yield return DoJumpUp();
                break;
            case RoboPathCore.RobotAction.JumpDown:
                yield return DoJumpDown();
                break;
            case RoboPathCore.RobotAction.Rock:
                yield return DoRock();
                break;
            case RoboPathCore.RobotAction.Zip:
                yield return DoZip();
                break;
            case RoboPathCore.RobotAction.Climb:
            case RoboPathCore.RobotAction.Crouch:
                yield return new WaitForSeconds(actionDuration);
                break;
        }
    }

    // Duck down, hold, rise back up
    private IEnumerator DoRock()
    {
        Vector3 origin = transform.position;
        Vector3 ducked = origin + new Vector3(0f, rockDuckY, 0f);

        // Duck down
        yield return LerpTo(origin, ducked, rockDuckDuration);

        // Hold low
        yield return new WaitForSeconds(rockHoldDuration);

        // Rise back
        yield return LerpTo(ducked, origin, rockRiseDuration);
    }

    // Fast forward slide with slight arc
    private IEnumerator DoZip()
    {
        Vector3 origin = transform.position;
        Vector3 end = origin + new Vector3(zipSlideX, 0f, 0f);

        float t = 0f;
        while (t < zipDuration)
        {
            t += Time.deltaTime;
            float p = t / zipDuration;
            transform.position = new Vector3(
                Mathf.Lerp(origin.x, end.x, p),
                origin.y + Mathf.Sin(p * Mathf.PI) * zipSlideY,
                origin.z);
            yield return null;
        }
        transform.position = end;
    }

    private IEnumerator DoJumpUp()
    {
        Vector3 origin = transform.position;
        Vector3 prep = origin + new Vector3(jumpPrepX, jumpPrepY, 0f);
        yield return LerpTo(origin, prep, jumpPrepDuration);

        Vector3 arcEnd = prep + new Vector3(jumpPrepX * 2f, jumpArcHeight, 0f);
        float t = 0f;
        while (t < jumpArcDuration)
        {
            t += Time.deltaTime;
            float p = t / jumpArcDuration;
            transform.position = new Vector3(
                Mathf.Lerp(prep.x, arcEnd.x, p),
                prep.y + Mathf.Sin(p * Mathf.PI) * jumpArcHeight,
                prep.z);
            yield return null;
        }
        transform.position = arcEnd;
    }

    private IEnumerator DoJumpDown()
    {
        Vector3 origin = transform.position;
        Vector3 prep = origin + new Vector3(jumpDownPrepX, jumpDownPrepY, 0f);
        yield return LerpTo(origin, prep, jumpDownPrepDuration);

        Vector3 arcEnd = prep + new Vector3(jumpDownPrepX * 2f, -jumpArcHeight, 0f);
        float t = 0f;
        while (t < jumpDownArcDuration)
        {
            t += Time.deltaTime;
            float p = t / jumpDownArcDuration;
            transform.position = new Vector3(
                Mathf.Lerp(prep.x, arcEnd.x, p),
                prep.y - Mathf.Sin(p * Mathf.PI * 0.5f) * jumpArcHeight,
                prep.z);
            yield return null;
        }
        transform.position = arcEnd;
    }

    public IEnumerator Shake()
    {
        Vector3 origin = transform.position;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float fade = 1f - (elapsed / shakeDuration);
            transform.position = origin + new Vector3(
                Mathf.Sin(elapsed * shakeFrequency) * shakeMagnitude * fade, 0f, 0f);
            yield return null;
        }
        transform.position = origin;
    }

    private IEnumerator LerpTo(Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        transform.position = to;
    }
}