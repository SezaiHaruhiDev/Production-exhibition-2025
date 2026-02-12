using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using Novel.Data;
using Novel.Managers;
using Common;

namespace Novel.System
{
    /// <summary>
    /// ノベルシステムのコマンドを実行（bg, ncr, ch, bgm, se など）
    /// </summary>
    public class CommandExecutor
    {
        private NovelEngine _manager;

        public CommandExecutor(NovelEngine manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// コマンドリストを順次実行する
        /// </summary>
        public void Execute(List<Command> commands)
        {
            foreach (var cmd in commands)
            {
                ProcessCommand(cmd);
            }
        }

        private void ProcessCommand(Command cmd)
        {
            switch (cmd.type)
            {
                case GameConstants.NovelCommands.Types.Background:
                    ChangeBackground(cmd);
                    break;
                case GameConstants.NovelCommands.Types.NewCharacter:
                    _manager.CharacterManager.CreateCharacter(cmd);
                    break;
                case GameConstants.NovelCommands.Types.ChangeCharacterSize:
                    _manager.CharacterManager.ChangeCharacterSize(cmd);
                    break;
                case GameConstants.NovelCommands.Types.ChangeCharacter:
                    _manager.CharacterManager.ChangeCharacter(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Choice:
                    _manager.StartCoroutine(_manager.ChoicesManager.ChoiceButton(cmd, (label) =>
                    {
                        _manager.GoToLabel(label);
                    }));
                    break;
                case GameConstants.NovelCommands.Types.BGM:
                    _manager.BgmManager.ProcessCommand(cmd);
                    break;
                case GameConstants.NovelCommands.Types.SE:
                    _manager.SeManager.ProcessCommand(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Affiliation:
                    ShowAffiliation(cmd);
                    break;
                case GameConstants.NovelCommands.Types.UI:
                    _manager.UiManager.UIChange(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Outline:
                    SetSubTextFlag(cmd);
                    break;
                case GameConstants.NovelCommands.Types.FocusCamera:
                    ControlCamera(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Wait:
                    Wait(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Speed:
                    ChangeSpeed(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Font:
                    ChangeFontSize(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Color:
                    ChangeFontColor(cmd);
                    break;
                case GameConstants.NovelCommands.Types.ToggleBackground:
                    SwitchImageState(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Click:
                    AutoClick(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Shake:
                    _manager.CameraFocusManager.Camera(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Skip:
                    SkipToLabel(cmd);
                    break;

                case GameConstants.NovelCommands.Types.Title:
                    ShowTitle(cmd);
                    break;
                case GameConstants.NovelCommands.Types.CheckFlags:
                    CheckFlags(cmd);
                    break;
                case GameConstants.NovelCommands.Types.BranchIf:
                    BranchIf(cmd);
                    break;
                case GameConstants.NovelCommands.Types.LoadScenario:
                    LoadScenario(cmd);
                    break;
                case GameConstants.NovelCommands.Types.Flag:
                    SetFlag(cmd);
                    break;
                case GameConstants.NovelCommands.Types.ResetFlags:
                    ResetFlags(cmd);
                    break;
                default:
                    Debug.LogWarning($"CommandExecutor: Unknown command type '{cmd.type}'");
                    break;
            }
        }

        private void ChangeBackground(Command cmd)
        {
            if (cmd.parameters.TryGetValue("sprite", out string spriteName))
            {
                Sprite sp = Resources.Load<Sprite>("sprites/" + spriteName);
                if (sp != null)
                {
                    _manager.BackgroundImage.sprite = sp;
                }
                else
                {
                    Debug.LogError($"Sprite not found: Sprites/{spriteName}");
                }
            }
        }

        private void ShowAffiliation(Command cmd)
        {
            if (cmd.parameters.TryGetValue("af", out string afistr))
            {
                _manager.AffiliationText.text = afistr;
            }
        }

        private void SetSubTextFlag(Command cmd)
        {
            if (cmd.parameters.TryGetValue("sub", out string subValue))
            {
                _manager.SubFlag = subValue == "true";
            }
        }

        private void ControlCamera(Command cmd)
        {
            if (cmd.parameters.TryGetValue("zoom", out string zoomValue))
            {
                if (zoomValue == "reset")
                {
                    _manager.CameraFocusManager.SetZoom(1f, 0.3f);
                }
                else if (float.TryParse(zoomValue, out float zoomScale))
                {
                    _manager.CameraFocusManager.SetZoom(zoomScale, 0.3f);
                }
                return;
            }

            if (cmd.parameters.TryGetValue("reset", out string nop))
            {
                _manager.CameraFocusManager.ResetFocus(0.5f);
                return;
            }

            _manager.CameraFocusManager.Camera(cmd);
        }

        private void Wait(Command cmd)
        {
            if (cmd.parameters.TryGetValue("time", out string t) && float.TryParse(t, out float sec))
            {
                _manager.StartCoroutine(_manager.DelayRoutine(sec));
            }
        }

        private void ChangeSpeed(Command cmd)
        {
            if (cmd.parameters.TryGetValue("value", out string spd) && float.TryParse(spd, out float newSpeed))
            {
                _manager.CaptionSpeed = newSpeed;
            }
        }

        private void ChangeFontSize(Command cmd)
        {
            if (cmd.parameters.TryGetValue("size", out string sizeStr) && float.TryParse(sizeStr, out float newSize))
            {
                _manager.SetFontSize(newSize);
            }
        }

        private void ChangeFontColor(Command cmd)
        {
            if (cmd.parameters.TryGetValue("r", out string rStr) &&
                cmd.parameters.TryGetValue("g", out string gStr) &&
                cmd.parameters.TryGetValue("b", out string bStr) &&
                float.TryParse(rStr, out float r) &&
                float.TryParse(gStr, out float g) &&
                float.TryParse(bStr, out float b))
            {
                Color newCol = new Color(r / 255f, g / 255f, b / 255f);
                _manager.SetFontColor(newCol);
            }
        }

        private void SwitchImageState(Command cmd)
        {
            if (_manager.SwitchImage != null && cmd.parameters.TryGetValue("state", out string st))
            {
                bool isB = st == "true";
                _manager.SwitchImage.sprite = isB ? _manager.ImageB : _manager.ImageA;
            }
        }

        private void AutoClick(Command cmd)
        {
            if (cmd.parameters.TryGetValue("value", out string value) && int.TryParse(value, out int cl))
            {
                for (int i = 0; i < cl; i++) _manager.OnClick();
            }
        }

        private void SkipToLabel(Command cmd)
        {
            if (cmd.parameters.TryGetValue("label", out string label))
            {
                _manager.GoToLabel(label);
            }
        }

        private void ShowTitle(Command cmd)
        {
            if (cmd.parameters.TryGetValue("name", out string sceneName))
            {
                _manager.ShowTitleSequence(sceneName);
            }
        }

        private void CheckFlags(Command cmd)
        {
            if (cmd.parameters.TryGetValue("ids", out string idsStr) && 
                cmd.parameters.TryGetValue("label", out string nextLabel))
            {
                string[] ids = idsStr.Split('^');
                if (_manager.FlagManager.CheckAll(ids))
                {
                    _manager.GoToLabel(nextLabel);
                }
            }
        }

        private void BranchIf(Command cmd)
        {
            if (cmd.parameters.TryGetValue("id", out string id) &&
                cmd.parameters.TryGetValue("label", out string nextLabel))
            {
                if (_manager.FlagManager.HasFlag(id))
                {
                    _manager.GoToLabel(nextLabel);
                }
            }
        }

        private void LoadScenario(Command cmd)
        {
            if (cmd.parameters.TryGetValue("file", out string fileName))
            {
                _manager.LoadNewScenario(fileName);
            }
        }

        private void SetFlag(Command cmd)
        {
            if (cmd.parameters.TryGetValue("id", out string id))
            {
                _manager.FlagManager.SetFlag(id);
            }
        }

        private void ResetFlags(Command cmd)
        {
            _manager.FlagManager.ResetAll();
        }
    }
}
