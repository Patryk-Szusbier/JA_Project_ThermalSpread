   M S B A M L   `   `   `  ���� H  DThermalSpread, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:ThermalSpread.componentsThermalSpread.components  C&clr-namespace:ThermalSpread.convertersThermalSpread.converters  .    (ThermalSpread.components.SimulationPanel   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     =
components&clr-namespace:ThermalSpread.components     =
converters&clr-namespace:ThermalSpread.converters     -     >�Name$  uc�� � 	Resources 9   3ThermalSpread.converters.BooleanVisibilityConverter   BooleanVisibilityConverter6   0ThermalSpread.converters.InverseBooleanConverter  InverseBooleanConverter2   ,ThermalSpread.converters.ComparisonConverter  ComparisonConverter��%-   &	        &	      &	 
           .���  � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � � ColumnDefinitions �� $
��Auto ��� $��0 ��� $��* �.+��� $
��Wrap=�$��0��$��0��$��Center=���`�  �� StringFormat$ Min step ms. = {0:N0}��.1���  �� Path$ 	MinStepMs� �� ElementName$ uc���� $��0��$��1��$��2��$��Center=� � Minimum$ 0Q� � Maximum$
 1000Q�	 �� TickFrequency$	 1Q�{��� $ IsSimulationRunning�
 �� Mode$
 OneWay=�$ uc�� �� 	Converter# [  � Value �� $ 	MinStepMs�$
 TwoWay=�$ uc���� $
��Wrap=�$��1��$��0��$��Center=���`� $ Nr of threads = {0:N0}��.1��� $ NrOfThreads�$ uc���� $��1��$��1��$��2��$��Center=�$ 1Q�$ 64Q�$	 1Q�{��� $ IsSimulationRunning�$
 OneWay=�$ uc��# [  �� $ NrOfThreads�$
 TwoWay=�$ uc��� $��2��$��3��{��� $ IsSimulationRunning�$
 OneWay=�$ uc��# [  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� .��DLL:�� $��
5, 0, 0, 0q�$��1��$��Center=�$��Center=� d� 	IsChecked �� $ SimulationTarget�$
 TwoWay=�# [  �� ConverterParameter$	 CPP��$ uc��.��C++�� $��
5, 0, 0, 0q�$��2��$��Center=�$��Center=� �� $ SimulationTarget�$
 TwoWay=�# [ $ C_SHARP��$ uc��.��C#�� $��
5, 0, 0, 0q�$��3��$��Center=�$��Center=� �� $ SimulationTarget�$
 TwoWay=�# [ $ 
ASM_VECTOR��$ uc��{��� $ IsAVX512Supported� �� RelativeSource ��  �� AncestorType UserControl.��ASM - vector�� -   $	��100��$��3��$��0��$��
0, 0, 5, 0q�$��Center=����#84c282
�������{��� $ IsSimulationRunning�$
 OneWay=�$ uc��# [   �� 
TargetType �� Property �� Value��%:   '��       �� " ��.���  �@��CornerRadius$ 15��.��START�� -   $	��100��$��3��$��0��$��
0, 0, 5, 0q�$��Center=����#f55d42
�������y��� $ IsSimulationRunning�$
 OneWay=�$ uc��# [   ��%:   '��       �� " ��.���  �@��CornerRadius$ 15��.��STOP�� $��3��$��2��$��Center=�{��� $ IsSimulationRunning�$
 OneWay=�$ uc��# [  �� $ UpdateUI�$
 TwoWay=�$ uc��.��	Update UI