   M S B A M L   `   `   `  ���� H  DThermalSpread, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ThermalSpreadThermalSpread  1    +ThermalSpread.components.TemperaturesCanvas   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     -localclr-namespace:ThermalSpread     -     >�Name$  uc��.����  �� CornerRadius$ 10��._��� -   .+��� -    ��Name$ matrixImage��$��0q�$
��Fill=� �� BitmapScalingMode$ NearestNeighbor=�����  �� RelativeSource �� *FindAncestor+ �� AncestorType" �� �� Path$ ActualWidth� �� Mode$ OneWay=�����  �� *FindAncestor+" ��$ ActualHeight�$ OneWay=��� -   $ drawingImage��$��0q�$
��Fill=�$ NearestNeighbor=�����  �� *FindAncestor+" ��$ ActualWidth�$ OneWay=�����  �� *FindAncestor+" ��$ ActualHeight�$ OneWay=��� 
�������$ 10�� �� Left$ 10��
	 �� Top$	 10��
 �� Padding$
 5q�{��� $ IsInteractivityEnabled� �� ElementName$ uc��$ OneWay=�._�]� .+��� $��/images/resize-icon.png��$��20��$��20��$��
0, 0, 5, 0q��� -    ��Name$ widthTxtBox��$��8(�$��Center=�$
��Bold"�$��0q�$��20�����#E5E5E5�� $��8(�$��Center=�$
��Bold"�.��x�� -   $ heightTxtBox��$��8(�$��Center=�$
��Bold"�$��0q�$��20�����#E5E5E5�� -    ��Name$ 
saveButton��$y�	Collapsed=�$��10, 0, 0, 0q�.���� $��20��$��/images/save-icon.png��