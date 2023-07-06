using UnityEngine;

[ExecuteInEditMode]
public class UpdateMaterialTime : MonoBehaviour
{
    private Material _material;

    [SerializeField]
    private float _timeScale = 1f;

    private float _time = 0f;

    private void Awake ()
    {
        _material = GetComponent<Renderer> ().sharedMaterials[0];
    }
    private void Update ()
    {
        _time += Time.deltaTime * _timeScale;
        _material.SetFloat ("Time", _time);
    }
}