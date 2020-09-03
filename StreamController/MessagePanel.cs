using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using uk.JohnCook.dotnet.MessageToImageLibrary;
using uk.JohnCook.dotnet.MessageToImageLibrary.Interop;

namespace uk.JohnCook.dotnet.StreamController
{
    class VerticalMessagePanel : IDisposable
    {
        private readonly SizeU panelSize = new SizeU() { Width = 1280, Height = 3408 };
        private readonly UInt32 panelBackgroundColor = (uint)System.Drawing.Color.Black.ToArgb();
        private readonly Direct2DWrapper direct2DWrapper = new Direct2DWrapper();
        private readonly MessagePanel verticalMessagePanel;

        public VerticalMessagePanel()
        {
            verticalMessagePanel = direct2DWrapper.CreateMessagePanel(panelSize, panelBackgroundColor);
            InitialiseMessagePanel();
            DrawVerticalTweet(@"G:\Program Files (x86)\mIRC\twimg\tmp\GMMH_NHS.jpg", "Greater Manchester Mental Health", "@GMMH_NHS", "A huge thank you to the wonderful team at HMP Hindley. You are all #GMMHSuperstars! 🌟🌟 #TogetherGMMH 💙", "Today, 13:42 UTC+1");
        }

        private void CreateBrushes()
        {
            verticalMessagePanel.CreateSolidColorBrush("borderBrush", (uint)System.Drawing.Color.DarkGray.ToArgb());
            verticalMessagePanel.CreateSolidColorBrush("backgroundBrush", (uint)System.Drawing.Color.Black.ToArgb());
            verticalMessagePanel.CreateSolidColorBrush("disablePixelBrush", 0xFF00FFFF);
            verticalMessagePanel.CreateSolidColorBrush("enablePixelBrush", 0xFF00FF00);
            verticalMessagePanel.CreateSolidColorBrush("textBrush", 0xFFFFFFFF);
        }

        private void InitialiseMessagePanel()
        {
            #region Set strings for header
            verticalMessagePanel.Header = "UK Tweets";
            verticalMessagePanel.SubHeader = "Tweets and Retweets from UK Resiliency Twitter Accounts";
            #endregion

            #region Set filenames and image sizes for resources
            verticalMessagePanel.SetImage(
                CanvasElement.NETWORK_LOGO,
                @"C:/Users/John/Pictures/Twitch/Twitter_Logo_Blue.png",
                240.0f,
                240.0f
                );
            verticalMessagePanel.SetImage(
                CanvasElement.SHARE_LOGO,
                @"C:/Users/John/Pictures/Twitch/Twitter_Retweet.png",
                240.0f - 100.0f,
                240.0f - 100.0f
                );
            #endregion

            #region Set Fonts
            verticalMessagePanel.SetFont(CanvasElement.HEADER, new FontSettings()
            {
                FontName = "Noto Sans",
                FontSize = 104.0f,
                FontWeight = 700,
                LocaleName = "en-GB",
                JustifyCentered = true
            });
            verticalMessagePanel.SetFont(CanvasElement.SUBHEADER, new FontSettings()
            {
                FontName = "Noto Sans",
                FontSize = 74.0f,
                FontWeight = 500,
                LocaleName = "en-GB",
                JustifyCentered = true
            });
            verticalMessagePanel.SetFont(CanvasElement.DISPLAY_NAME, new FontSettings()
            {
                FontName = "Noto Sans",
                FontSize = 82.0f,
                FontWeight = 700,
                LocaleName = "en-GB"
            });
            verticalMessagePanel.SetFont(CanvasElement.USERNAME, new FontSettings()
            {
                FontName = "Noto Sans",
                FontSize = 82.0f,
                FontWeight = 500,
                LocaleName = "en-GB"
            });
            verticalMessagePanel.SetFont(CanvasElement.TEXT, new FontSettings()
            {
                FontName = "Segoe UI Emoji",
                FontSize = 90.0f,
                FontWeight = 700,
                LocaleName = "en-GB"
            });
            verticalMessagePanel.SetFont(CanvasElement.TIME, new FontSettings()
            {
                FontName = "Noto Sans",
                FontSize = verticalMessagePanel.NetworkLogoRectangle.Bottom / 3,
                FontWeight = 700,
                LocaleName = "en-GB"
            });
            verticalMessagePanel.SetFont(CanvasElement.SHARER_DISPLAY_NAME, new FontSettings()
            {
                FontName = "Noto Sans",
                FontSize = verticalMessagePanel.NetworkLogoRectangle.Bottom / 3,
                FontWeight = 700,
                LocaleName = "en-GB"
            });
            verticalMessagePanel.SetFont(CanvasElement.SHARER_USERNAME, new FontSettings()
            {
                FontName = "Noto Sans",
                FontSize = verticalMessagePanel.NetworkLogoRectangle.Bottom / 3,
                FontWeight = 500,
                LocaleName = "en-GB"
            });
            #endregion

            #region Draw canvas with header

            #region Start drawing to canvas
            verticalMessagePanel.BeginDraw();
            #endregion

            #region Create brushes
            CreateBrushes();
            #endregion

            #region Draw Heading
            verticalMessagePanel.HeaderOriginPoint = new PointF()
            {
                X = 40.0f,
                Y = 40.0f
            };
            verticalMessagePanel.CreateTextLayout(CanvasElement.HEADER, new RectF()
            {
                Left = 0.0f,
                Top = 0.0f,
                Right = verticalMessagePanel.PanelRectangle.Width - (verticalMessagePanel.HeaderOriginPoint.X * 2),
                Bottom = verticalMessagePanel.PanelRectangle.Height - (verticalMessagePanel.HeaderOriginPoint.Y * 2)
            });
            verticalMessagePanel.DrawTextLayout(CanvasElement.HEADER, verticalMessagePanel.Brushes["textBrush"]);
            #endregion

            #region Draw SubHeading
            verticalMessagePanel.SubHeaderOriginPoint = new PointF()
            {
                X = 40.0f,
                Y = verticalMessagePanel.HeaderOriginPoint.Y + verticalMessagePanel.HeaderRectangle.Bottom
            };
            verticalMessagePanel.CreateTextLayout(CanvasElement.SUBHEADER, new RectF()
            {
                Left = 0.0f,
                Top = 0.0f,
                Right = verticalMessagePanel.PanelRectangle.Width - (verticalMessagePanel.SubHeaderOriginPoint.X * 2),
                Bottom = verticalMessagePanel.PanelRectangle.Height - (verticalMessagePanel.SubHeaderOriginPoint.Y * 2)
            });
            verticalMessagePanel.DrawTextLayout(CanvasElement.SUBHEADER, verticalMessagePanel.Brushes["textBrush"]);
            #endregion

            #region Draw Heading Separator
            verticalMessagePanel.HeadingSeparatorPoint1 = new PointF()
            {
                X = 40.0f,
                Y = verticalMessagePanel.SubHeaderOriginPoint.Y + verticalMessagePanel.SubHeaderRectangle.Bottom + 60.0f
            };
            verticalMessagePanel.HeadingSeparatorPoint2 = new PointF()
            {
                X = verticalMessagePanel.PanelRectangle.Width - (verticalMessagePanel.HeadingSeparatorPoint1.X * 2),
                Y = verticalMessagePanel.HeadingSeparatorPoint1.Y
            };
            verticalMessagePanel.DrawHeadingSeparator(verticalMessagePanel.Brushes["textBrush"], 8.0f);
            #endregion

            #region Finish drawing to canvas
            verticalMessagePanel.EndDraw();
            #endregion

            #region Set Tweet area relative to canvas
            verticalMessagePanel.MessageOriginPoint = new PointF()
            {
                X = 60.0f,
                Y = verticalMessagePanel.HeadingSeparatorRectangle.Bottom + 90.0f
            };
            verticalMessagePanel.MessageRectangle = new RectF()
            {
                Left = 0,
                Top = 0,
                Right = verticalMessagePanel.PanelRectangle.Width - (verticalMessagePanel.MessageOriginPoint.X * 2),
                Bottom = verticalMessagePanel.PanelRectangle.Height - verticalMessagePanel.MessageOriginPoint.Y - 60.0f
            };
            #endregion

            #region Set Tweet profile image area relative to canvas
            verticalMessagePanel.ProfileImageOriginPoint = new PointF()
            {
                X = verticalMessagePanel.MessageOriginPoint.X,
                Y = verticalMessagePanel.MessageOriginPoint.Y
            };
            verticalMessagePanel.ProfileImageRectangle = new RectF()
            {
                Left = 0.0f,
                Top = 0.0f,
                Right = 240.0f,
                Bottom = 240.0f
            };
            #endregion

            #region Set display name origin point relative to canvas
            verticalMessagePanel.DisplayNameOriginPoint = new PointF()
            {
                X = (verticalMessagePanel.ProfileImageOriginPoint.X * 2) + verticalMessagePanel.ProfileImageRectangle.Right,
                Y = verticalMessagePanel.ProfileImageOriginPoint.Y
            };
            #endregion

            #endregion
        }

        private string DrawVerticalTweet(string profileImageFilename, string displayName, string username, string text, string time, string retweeterDisplayName = null, string retweeterUsername = null)
        {
            verticalMessagePanel.ClearArea(verticalMessagePanel.MessageOriginPoint, verticalMessagePanel.MessageRectangle, verticalMessagePanel.Brushes["backgroundBrush"], true, true);

            #region Profile image
            // TODO: Set ProfileImageFilename
            verticalMessagePanel.SetImage(
                CanvasElement.PROFILE_IMAGE,
                profileImageFilename,
                240.0f,
                240.0f
                );

            // TODO: Create and draw round profile image
            verticalMessagePanel.PushCircleLayer(CanvasElement.PROFILE_IMAGE, verticalMessagePanel.Brushes["enablePixelBrush"]);
            verticalMessagePanel.DrawImage(CanvasElement.PROFILE_IMAGE);
            verticalMessagePanel.PopLayer();
            #endregion

            #region Start drawing to canvas
            verticalMessagePanel.BeginDraw();
            #endregion

            #region Display name and username
            // TODO: Get DisplayName and set DisplayNameRectangle
            verticalMessagePanel.DisplayName = displayName;
            verticalMessagePanel.CreateTextLayout(CanvasElement.DISPLAY_NAME, new RectF()
            {
                Left = 0.0f,
                Top = 0.0f,
                Right = verticalMessagePanel.MessageRectangle.Right - verticalMessagePanel.ProfileImageRectangle.Right - verticalMessagePanel.ProfileImageOriginPoint.X,
                Bottom = verticalMessagePanel.ProfileImageRectangle.Bottom / 2
            });
            verticalMessagePanel.DrawTextLayout(CanvasElement.SUBHEADER, verticalMessagePanel.Brushes["textBrush"]);

            // TODO: Get Username and set UsernameRectangle
            verticalMessagePanel.Username = username;
            verticalMessagePanel.CreateTextLayout(CanvasElement.USERNAME, new RectF()
            {
                Left = 0.0f,
                Top = 0.0f,
                Right = verticalMessagePanel.MessageRectangle.Right - verticalMessagePanel.ProfileImageRectangle.Right - verticalMessagePanel.ProfileImageOriginPoint.X,
                Bottom = verticalMessagePanel.ProfileImageRectangle.Bottom / 2
            });

            // TODO: Calculate and set DisplayNameOriginPoint and UsernameOriginPoint
            if (verticalMessagePanel.ProfileImageRectangle.Bottom / 2 >= verticalMessagePanel.DisplayNameRectangle.Bottom)
            {
                verticalMessagePanel.UsernameOriginPoint = new PointF()
                {
                    X = verticalMessagePanel.DisplayNameOriginPoint.X,
                    Y = verticalMessagePanel.MessageOriginPoint.Y + (verticalMessagePanel.ProfileImageRectangle.Bottom / 2)
                };
            }
            else
            {
                verticalMessagePanel.UsernameOriginPoint = new PointF()
                {
                    X = verticalMessagePanel.DisplayNameOriginPoint.X,
                    Y = verticalMessagePanel.DisplayNameOriginPoint.Y + verticalMessagePanel.DisplayNameRectangle.Bottom
                };
            }
            verticalMessagePanel.DrawTextLayout(CanvasElement.DISPLAY_NAME, verticalMessagePanel.Brushes["textBrush"]);
            verticalMessagePanel.DrawTextLayout(CanvasElement.USERNAME, verticalMessagePanel.Brushes["textBrush"]);
            #endregion

            #region Tweet text
            // TODO: Get TweetText and set TweetTextRectangle
            verticalMessagePanel.MessageText = text;
            verticalMessagePanel.CreateTextLayout(CanvasElement.TEXT, new RectF()
            {
                Left = 0.0f,
                Top = 0.0f,
                Right = verticalMessagePanel.MessageRectangle.Right,
                Bottom = verticalMessagePanel.MessageRectangle.Bottom - verticalMessagePanel.UsernameRectangle.Bottom
            });
            // TODO: Calculate and set TweetTextOriginPoint
            verticalMessagePanel.MessageTextOriginPoint = new PointF()
            {
                X = verticalMessagePanel.MessageOriginPoint.X,
                Y = verticalMessagePanel.ProfileImageOriginPoint.Y + Math.Max(verticalMessagePanel.ProfileImageRectangle.Bottom, verticalMessagePanel.DisplayNameRectangle.Bottom + verticalMessagePanel.UsernameRectangle.Bottom) + 90.0f
            };
            verticalMessagePanel.DrawTextLayout(CanvasElement.TEXT, verticalMessagePanel.Brushes["textBrush"]);
            #endregion

            #region Twitter logo and time
            // TODO: Calculate and set TwitterLogoOriginPoint
            verticalMessagePanel.NetworkLogoOriginPoint = new PointF()
            {
                X = verticalMessagePanel.MessageOriginPoint.X,
                Y = verticalMessagePanel.MessageTextOriginPoint.Y + verticalMessagePanel.MessageTextRectangle.Bottom + 60.0f
            };
            verticalMessagePanel.DrawImage(CanvasElement.NETWORK_LOGO);

            // TODO: Get TweetTime and set TweetTimeRectangle
            verticalMessagePanel.Time = time;
            verticalMessagePanel.CreateTextLayout(CanvasElement.TIME, new RectF()
            {
                Left = 0.0f,
                Top = 0.0f,
                Right = verticalMessagePanel.MessageRectangle.Right - verticalMessagePanel.NetworkLogoRectangle.Right,
                Bottom = verticalMessagePanel.NetworkLogoRectangle.Bottom
            });

            // TODO: Calculate and set TweetTimeOriginPoint
            verticalMessagePanel.TimeOriginPoint = new PointF()
            {
                X = verticalMessagePanel.MessageOriginPoint.X + verticalMessagePanel.NetworkLogoRectangle.Right,
                Y = verticalMessagePanel.TimeRectangle.Bottom < verticalMessagePanel.NetworkLogoRectangle.Bottom
                    ? verticalMessagePanel.NetworkLogoOriginPoint.Y + ((verticalMessagePanel.NetworkLogoRectangle.Bottom - verticalMessagePanel.TimeRectangle.Bottom) / 2)
                    : verticalMessagePanel.NetworkLogoOriginPoint.Y
            };
            verticalMessagePanel.DrawTextLayout(CanvasElement.TIME, verticalMessagePanel.Brushes["textBrush"]);
            #endregion

            #region Retweet logo and Retweeter display name & username
            if (retweeterDisplayName != null && retweeterUsername != null)
            {
                // TODO: Calculate and set RetweetLogoOriginPoint
                verticalMessagePanel.ShareLogoOriginPoint = new PointF()
                {
                    X = verticalMessagePanel.MessageOriginPoint.X + 50.0f,
                    Y = verticalMessagePanel.NetworkLogoOriginPoint.Y + Math.Max(verticalMessagePanel.NetworkLogoRectangle.Bottom, verticalMessagePanel.TimeRectangle.Bottom) + 50.0f
                };
                verticalMessagePanel.DrawImage(CanvasElement.SHARE_LOGO);

                // TODO: Get RetweeterDisplayName and set RetweeterDisplayNameRectangle
                verticalMessagePanel.SharerDisplayName = retweeterDisplayName;
                verticalMessagePanel.CreateTextLayout(CanvasElement.SHARER_DISPLAY_NAME, new RectF()
                {
                    Left = 0.0f,
                    Top = 0.0f,
                    Right = verticalMessagePanel.MessageRectangle.Right - verticalMessagePanel.ShareLogoRectangle.Right - 100.0f,
                    Bottom = verticalMessagePanel.ShareLogoRectangle.Bottom
                });
                // TODO: Calculate and set RetweeterDisplayNameOriginPoint
                verticalMessagePanel.SharerDisplayNameOriginPoint = new PointF()
                {
                    X = verticalMessagePanel.MessageOriginPoint.X + verticalMessagePanel.ShareLogoRectangle.Right + 100.0f,
                    Y = verticalMessagePanel.SharerDisplayNameRectangle.Bottom < verticalMessagePanel.ShareLogoRectangle.Bottom
                    ? verticalMessagePanel.ShareLogoOriginPoint.Y + ((verticalMessagePanel.ShareLogoRectangle.Bottom - verticalMessagePanel.SharerDisplayNameRectangle.Bottom) / 2)
                    : verticalMessagePanel.ShareLogoOriginPoint.Y
                };

                // TODO: Get RetweeterUsername and set RetweeterUsernameRectangle
                verticalMessagePanel.SharerUsername = $"({retweeterUsername})";
                verticalMessagePanel.CreateTextLayout(CanvasElement.SHARER_USERNAME, new RectF()
                {
                    Left = 0.0f,
                    Top = 0.0f,
                    Right = verticalMessagePanel.MessageRectangle.Right - verticalMessagePanel.ShareLogoRectangle.Right,
                    Bottom = verticalMessagePanel.ShareLogoRectangle.Bottom
                });
                // TODO: Calculate and set RetweeterUsernameOriginPoint
                verticalMessagePanel.SharerUsernameOriginPoint = new PointF()
                {
                    X = verticalMessagePanel.SharerDisplayNameOriginPoint.X,
                    Y = verticalMessagePanel.ShareLogoOriginPoint.Y + Math.Max(verticalMessagePanel.ShareLogoRectangle.Bottom, verticalMessagePanel.SharerDisplayNameRectangle.Bottom)
                };
                verticalMessagePanel.DrawTextLayout(CanvasElement.SHARER_DISPLAY_NAME, verticalMessagePanel.Brushes["textBrush"]);
                verticalMessagePanel.DrawTextLayout(CanvasElement.SHARER_USERNAME, verticalMessagePanel.Brushes["textBrush"]);
            }
            #endregion

            #region Finish drawing to canvas
            verticalMessagePanel.EndDraw();
            #endregion

            #region Save Image
            // Date/Time format to append to file name. Custom format because filenames cannot contain colons.
            DateTimeFormat dateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHHmmss.fffffffZ");
            // Set file name to a deterministic but unlikely to be duplicated name using executable name and datetime, e.g. TextFormatter_2020-08-27T181638.7742753Z.PNG
            string fileName = Assembly.GetEntryAssembly().GetName().Name + "_" + DateTime.UtcNow.ToString(dateTimeFormat.FormatString, CultureInfo.InvariantCulture) + ".PNG";
            // Set save location to %TEMP%
            string saveLocation = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
            try
            {
                verticalMessagePanel.SaveImage(saveLocation);
                Trace.WriteLine($"Image successfully saved to {saveLocation}");
                // Open file explorer with the saved file selected
                string selectFileArgument = $"/select, \"{saveLocation}\"";
                Process.Start("explorer.exe", selectFileArgument);
                return saveLocation;
            }
            catch (FileNotFoundException e)
            {
                Trace.WriteLine($"Error saving to file {saveLocation}: {e.Message} - {e.InnerException?.Message}");
                return null;
            }
            #endregion
        }

        public void Dispose()
        {
            direct2DWrapper.Dispose();
        }
    }
}
