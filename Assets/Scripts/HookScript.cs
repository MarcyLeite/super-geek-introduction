using UnityEngine;

public class HookScript : MonoBehaviour
{
    public SpriteRenderer target;
    void Start() {
        target.enabled = false;
    }

    public void ToggleTarget(bool value) {
        target.enabled = value;
    }
}
