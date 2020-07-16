using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RubberLeash : MVRScript
{
    private readonly List<string> _emptyStringsList = new List<string>();

    private JSONStorableStringChooser _targetControllerJSON;
    private JSONStorableStringChooser _parentAtomJSON;
    private JSONStorableStringChooser _parentControllerJSON;
    public JSONStorableStringChooser _parentTransformJSON;
    private JSONStorableFloat _weight;
    private JSONStorableFloat _posX;
    private JSONStorableFloat _posY;
    private JSONStorableFloat _posZ;
    private JSONStorableFloat _rotX;
    private JSONStorableFloat _rotY;
    private JSONStorableFloat _rotZ;
    private JSONStorableFloat _rotW;

    private FreeControllerV3 _targetController;
    private FreeControllerV3 _parentController;
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
            _targetControllerJSON = new JSONStorableStringChooser("Target Controller", containingAtom.freeControllers.Select(fc => fc.name).ToList(), containingAtom.freeControllers.FirstOrDefault()?.name, "Target Controller", OnControllerUpdated);
            RegisterStringChooser(_targetControllerJSON);
            CreateScrollablePopup(_targetControllerJSON, false);

            _parentAtomJSON = new JSONStorableStringChooser("Parent Atom", _emptyStringsList, null, "Parent Atom", SyncDropDowns);
            RegisterStringChooser(_parentAtomJSON);
            CreateScrollablePopup(_parentAtomJSON, false);

            _parentControllerJSON = new JSONStorableStringChooser("Parent Controller", SuperController.singleton.GetAtomUIDs(), null, "Parent Controller", OnParentUpdated);
            RegisterStringChooser(_parentControllerJSON);
            CreateScrollablePopup(_parentControllerJSON, false);

            _parentTransformJSON = new JSONStorableStringChooser("Parent Transform", new List<string> { _both, _rotationOnly, _positionOnly, _none }, _both, "Parent Transform");
            RegisterStringChooser(_parentTransformJSON);
            CreateScrollablePopup(_parentTransformJSON, false);

            _weight = new JSONStorableFloat("Weight", 0f, 0f, 1f, true);
            RegisterFloat(_weight);
            CreateSlider(_weight, true);

            _posX = new JSONStorableFloat("PosX", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_posX);
            _posY = new JSONStorableFloat("PosY", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_posY);
            _posZ = new JSONStorableFloat("PosZ", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_posZ);
            _rotX = new JSONStorableFloat("RotX", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_rotX);
            _rotY = new JSONStorableFloat("RotY", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_rotY);
            _rotZ = new JSONStorableFloat("RotZ", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_rotZ);
            _rotW = new JSONStorableFloat("RotW", 0f, OnOffsetUpdated, -10f, 10f, false);
            RegisterFloat(_rotW);

            _recordButton = CreateButton("Record current position");
            _recordButton.button.onClick.AddListener(OnRecordCurrentPosition);
            _recordButton.button.interactable = false;

            OnControllerUpdated(_targetControllerJSON.val);
            SyncDropDowns(_parentAtomJSON.val);
            OnParentUpdated(_parentControllerJSON.val);
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
            _parentControllerJSON.choices = _emptyStringsList;
            if (_parentControllerJSON.val != null)
            {
                _parentControllerJSON.val = null;
            }
        }
        else
        {
            _parentControllerJSON.choices = atom.freeControllers.Select(fc => fc.name).ToList();
            if (!_parentControllerJSON.choices.Contains(_parentControllerJSON.val))
            {
                _parentControllerJSON.val = _parentControllerJSON.choices.FirstOrDefault();
            }
        }
    }

    private void OnControllerUpdated(string val)
    {
        _targetController = containingAtom.freeControllers.FirstOrDefault(fc => fc.name == val);
        _recordButton.button.interactable = _targetController != null && _parentController != null;
    }

    private void OnParentUpdated(string val)
    {
        var atom = _parentAtomJSON.val != null ? SuperController.singleton.GetAtomByUid(_parentAtomJSON.val) : null;
        if (atom == null)
        {
            _parentController = null;
            _recordButton.button.interactable = false;
            return;
        }
        var controller = atom.freeControllers.FirstOrDefault(fc => fc.name == val);
        if (controller == null)
        {
            _parentController = null;
            _recordButton.button.interactable = false;
            return;
        }
        _parentController = controller;
        _recordButton.button.interactable = _targetController != null && _parentController != null;
    }

    private void OnOffsetUpdated(float _)
    {
        _localPosition = new Vector3(_posX.val, _posY.val, _posZ.val);
        _localRotation = new Quaternion(_rotX.val, _rotY.val, _rotZ.val, _rotZ.val);
    }

    private void OnRecordCurrentPosition()
    {
        if (_parentController == null || _targetController == null) return;
        _localPosition = _parentController.transform.InverseTransformPoint(_targetController.transform.position);
        _localRotation = Quaternion.Inverse(_parentController.transform.rotation) * _targetController.transform.rotation;
        _posX.valNoCallback = _localPosition.x;
        _posY.valNoCallback = _localPosition.y;
        _posZ.valNoCallback = _localPosition.z;
        _rotX.valNoCallback = _localRotation.x;
        _rotY.valNoCallback = _localRotation.y;
        _rotZ.valNoCallback = _localRotation.z;
        _rotW.valNoCallback = _localRotation.w;
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
        OnParentUpdated(_parentControllerJSON.val);
    }

    public void FixedUpdate()
    {
        if (_weight.val == 0f || _parentController == null || _targetController == null || _parentTransformJSON.val == _none) return;
        try
        {
            // TODO: Cache
            var rb = _targetController.GetComponent<Rigidbody>();
            if (_parentTransformJSON.val == _both || _parentTransformJSON.val == _positionOnly)
                rb.MovePosition(Vector3.Slerp(_targetController.transform.position, _parentController.transform.position + _localPosition, _weight.val));
            if (_parentTransformJSON.val == _both || _parentTransformJSON.val == _rotationOnly)
                rb.MoveRotation(Quaternion.Slerp(_targetController.transform.rotation, _parentController.transform.rotation * _localRotation, _weight.val));
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(RubberLeash)}.{nameof(FixedUpdate)}: {e}");
            _weight.val = 0f;
        }
    }
}
