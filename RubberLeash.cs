using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RubberLeash : MVRScript
{
    private readonly List<string> _emptyStringsList = new List<string>();

    private JSONStorableStringChooser _targetRigidbodyJSON;
    private JSONStorableStringChooser _parentAtomJSON;
    private JSONStorableStringChooser _parentRigidbodyJSON;
    public JSONStorableStringChooser _parentTransformJSON;
    private JSONStorableFloat _weightJSON;
    private JSONStorableFloat _posXJSON;
    private JSONStorableFloat _posYJSON;
    private JSONStorableFloat _posZJSON;
    private JSONStorableFloat _rotXJSON;
    private JSONStorableFloat _rotYJSON;
    private JSONStorableFloat _rotZJSON;
    private JSONStorableFloat _rotWJSON;

    private Rigidbody _targetRigidbody;
    private Rigidbody _parentRigidbody;
    private Vector3 _localPosition;
    private Quaternion _localRotation;
    private UIDynamicButton _recordButton;
    private const string _both = "Both";
    private const string _rotationOnly = "Rotation Only";
    private const string _positionOnly = "Position Only";
    private const string _none = "None";


    public override void Init()
    {
        try
        {
            _targetRigidbodyJSON = new JSONStorableStringChooser("Target Rgidbody", containingAtom.linkableRigidbodies.Select(rb => rb.name).ToList(), containingAtom.linkableRigidbodies.FirstOrDefault()?.name, "Target Rgidbody", OnTargetRigidbodyUpdated);
            RegisterStringChooser(_targetRigidbodyJSON);
            CreateScrollablePopup(_targetRigidbodyJSON, false).popupPanelHeight = 1000;

            _parentAtomJSON = new JSONStorableStringChooser("Parent Atom", _emptyStringsList, null, "Parent Atom", SyncDropDowns);
            RegisterStringChooser(_parentAtomJSON);
            CreateScrollablePopup(_parentAtomJSON, false).popupPanelHeight = 900;

            _parentRigidbodyJSON = new JSONStorableStringChooser("Parent Rigidbody", _emptyStringsList, null, "Parent Rigidbody", OnParentRigidbodyUpdated);
            RegisterStringChooser(_parentRigidbodyJSON);
            CreateScrollablePopup(_parentRigidbodyJSON, false).popupPanelHeight = 800;

            _parentTransformJSON = new JSONStorableStringChooser("Parent Transform", new List<string> { _both, _rotationOnly, _positionOnly, _none }, _both, "Parent Transform");
            RegisterStringChooser(_parentTransformJSON);
            CreateScrollablePopup(_parentTransformJSON, false).popupPanelHeight = 300;

            _weightJSON = new JSONStorableFloat("Weight", 0f, 0f, 1f, true);
            RegisterFloat(_weightJSON);
            CreateSlider(_weightJSON, true);

            _posXJSON = new JSONStorableFloat("PosX", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_posXJSON);
            _posYJSON = new JSONStorableFloat("PosY", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_posYJSON);
            _posZJSON = new JSONStorableFloat("PosZ", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_posZJSON);
            _rotXJSON = new JSONStorableFloat("RotX", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_rotXJSON);
            _rotYJSON = new JSONStorableFloat("RotY", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_rotYJSON);
            _rotZJSON = new JSONStorableFloat("RotZ", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_rotZJSON);
            _rotWJSON = new JSONStorableFloat("RotW", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_rotWJSON);

            _recordButton = CreateButton("Record current position");
            _recordButton.button.onClick.AddListener(OnRecordCurrentPosition);
            _recordButton.button.interactable = false;

            OnTargetRigidbodyUpdated(_targetRigidbodyJSON.val);
            SyncDropDowns(_parentAtomJSON.val);
            OnParentRigidbodyUpdated(_parentRigidbodyJSON.val);
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(RubberLeash)}.{nameof(Init)}: {e}");
        }
    }

    private void SyncDropDowns(string val)
    {
        _parentAtomJSON.choices = SuperController.singleton.GetAtomUIDs();
        var atom = _parentAtomJSON.val != null ? SuperController.singleton.GetAtomByUid(_parentAtomJSON.val) : null;
        if (atom == null)
        {
            _parentRigidbodyJSON.choices = _emptyStringsList;
            if (_parentRigidbodyJSON.val != null)
            {
                _parentRigidbodyJSON.val = null;
            }
        }
        else
        {
            _parentRigidbodyJSON.choices = atom.linkableRigidbodies.Select(rb => rb.name).ToList();
            if (!_parentRigidbodyJSON.choices.Contains(_parentRigidbodyJSON.val))
            {
                _parentRigidbodyJSON.val = _parentRigidbodyJSON.choices.FirstOrDefault();
            }
        }
    }

    private void OnTargetRigidbodyUpdated(string val)
    {
        _targetRigidbody = containingAtom.linkableRigidbodies.FirstOrDefault(rb => rb.name == val);
        _recordButton.button.interactable = _targetRigidbody != null && _parentRigidbody != null;
    }

    private void OnParentRigidbodyUpdated(string val)
    {
        var atom = _parentAtomJSON.val != null ? SuperController.singleton.GetAtomByUid(_parentAtomJSON.val) : null;
        if (atom == null)
        {
            _parentRigidbody = null;
            _recordButton.button.interactable = false;
            return;
        }
        var controller = atom.linkableRigidbodies.FirstOrDefault(rb => rb.name == val);
        if (controller == null)
        {
            _parentRigidbody = null;
            _recordButton.button.interactable = false;
            return;
        }
        _parentRigidbody = controller.GetComponent<Rigidbody>();
        _recordButton.button.interactable = _targetRigidbody != null && _parentRigidbody != null;
    }

    private void OnOffsetUpdated(float _)
    {
        _localPosition = new Vector3(_posXJSON.val, _posYJSON.val, _posZJSON.val);
        _localRotation = new Quaternion(_rotXJSON.val, _rotYJSON.val, _rotZJSON.val, _rotZJSON.val);
    }

    private void OnRecordCurrentPosition()
    {
        if (_parentRigidbody == null || _targetRigidbody == null) return;
        _localPosition = _parentRigidbody.transform.InverseTransformPoint(_targetRigidbody.position);
        _localRotation = Quaternion.Inverse(_parentRigidbody.rotation) * _targetRigidbody.rotation;
        _posXJSON.valNoCallback = _localPosition.x;
        _posYJSON.valNoCallback = _localPosition.y;
        _posZJSON.valNoCallback = _localPosition.z;
        _rotXJSON.valNoCallback = _localRotation.x;
        _rotYJSON.valNoCallback = _localRotation.y;
        _rotZJSON.valNoCallback = _localRotation.z;
        _rotWJSON.valNoCallback = _localRotation.w;
    }

    public void OnEnable()
    {
        try
        {
            SuperController.singleton.onAtomUIDsChangedHandlers += OnAtomUIDsChanged;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(RubberLeash)}.{nameof(OnEnable)}: {e}");
        }
    }

    public void OnDisable()
    {
        try
        {
            SuperController.singleton.onAtomUIDsChangedHandlers -= OnAtomUIDsChanged;
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(RubberLeash)}.{nameof(OnDisable)}: {e}");
        }
    }

    private void OnAtomUIDsChanged(List<string> _)
    {
        SyncDropDowns(_parentAtomJSON.val);
        OnParentRigidbodyUpdated(_parentRigidbodyJSON.val);
    }

    public void FixedUpdate()
    {
        if (_weightJSON.val == 0f || _parentRigidbody == null || _targetRigidbody == null || _parentTransformJSON.val == _none) return;
        try
        {
            if (_parentTransformJSON.val == _both || _parentTransformJSON.val == _rotationOnly)
                _targetRigidbody.MoveRotation(Quaternion.Slerp(_targetRigidbody.rotation, _parentRigidbody.rotation * _localRotation, _weightJSON.val));
            if (_parentTransformJSON.val == _both || _parentTransformJSON.val == _positionOnly)
                _targetRigidbody.MovePosition(Vector3.Slerp(_targetRigidbody.position, _parentRigidbody.transform.TransformPoint(_localPosition), _weightJSON.val));
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(RubberLeash)}.{nameof(FixedUpdate)}: {e}");
            _weightJSON.val = 0f;
        }
    }
}
