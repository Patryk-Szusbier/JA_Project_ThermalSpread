   M S B A M L   `   `   `  ����H  DThermalSpread, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:ThermalSpread.componentsThermalSpread.components  5      C&clr-namespace:ThermalSpread.convertersThermalSpread.converters  5      .    (ThermalSpread.components.SimulationPanel   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      =
components&clr-namespace:ThermalSpread.components     5      =
converters&clr-namespace:ThermalSpread.converters     5      -   5          >�Name$  uc��5	       � 	Resources 5
      9   3ThermalSpread.converters.BooleanVisibilityConverter   BooleanVisibilityConverter6   0ThermalSpread.converters.InverseBooleanConverter  InverseBooleanConverter2   ,ThermalSpread.converters.ComparisonConverter  ComparisonConverter��%W   &	        &	      &	 &       5   
   60     5   
   6-     5   
   6)   5      .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��0 �6   �� 5      $��* �6   .+�5!   
   �� $
��Wrap=�6   $��0��6(   $��0��65   $��Center=�6E   ��5"      `� 5#       �� StringFormat$ Min step ms. = {0:N0}��6   .1�5$      ��  �� Path$ 	MinStepMs�6    �� ElementName$ uc��6/   5%      5'      �� 5(   
   $��0��6�   $��1��6�   $��2��6�   $��Center=�6�    � Minimum$ 0Q�6�    � Maximum$
 1000Q�6�   	 �� TickFrequency$	 1Q�66  {�6   �� $ IsSimulationRunning�
 �� Mode$
 OneWay=�$ uc�� �� 	Converter# [  � Value 6�   �� $ 	MinStepMs�$
 TwoWay=�$ uc��66  �� 5*   
   $
��Wrap=�6   $��1��6(   $��0��65   $��Center=�6E   ��5+      `� 5,      $ Nr of threads = {0:N0}��6   .1�5-      �� $ NrOfThreads�6   $ uc��61   5.      50      �� 51   
   $��1��6   $��1��6   $��2��6.   $��Center=�6B   $ 1Q�6]   $ 20Q�6i   $	 1Q�6�    6v   �� $ NrOfThreads�$
 TwoWay=�$ uc��6�   � 53   
   $��2��6   $��3��6   {�60   �� $ IsSimulationRunning�$
 OneWay=�$ uc��# [  54      �� 55      $
��Auto �6#   �� 56      $
��Auto �6#   �� 57      $
��Auto �6#   .+�5:      �� .��6   DLL:6   �� 5;      $��
5, 0, 0, 0q�6   $��1��6�   $��Center=�6�   $��Center=�6�    d� 	IsChecked 6+   �� $ SimulationTarget�$
 TwoWay=�# [  �� ConverterParameter$ C_SHARP��$ uc��.��6  C#6  �� 5<      $��
5, 0, 0, 0q�6   $��2��6u   $��Center=�6�   $��Center=�6�    6+   �� $ SimulationTarget�$
 TwoWay=�$ uc��.��6�   ASM - vector6�   5=      �� 5>   
   -   5        $	��100��5>      $��3��6   $��0��6+   $��
0, 0, 5, 0q�6;   $��Center=�6O   ���#84c282
������� 5?       �� 
TargetType �� Property �� Value��%_   '��       �� 5@      " ��6   .�5A      ��  �@��CornerRadius$ 15��65   5B      5C      .��6    START5F      �� 5H   
   -   5        $	��100��5H      $��3��6�   $��0��6�   $��
0, 0, 5, 0q�6�   $��Center=�6�   ���#cc0000
�������y�6   �� $ IsSimulationRunning�$
 OneWay=�$ uc��# [   5I      ��%_   '��       �� 5J      " ��6   .�5K      ��  �@��CornerRadius$ 10��65   5L      5M      .��6    STOP5P      �� 5R   
   $��3��6�   $��2��6�   $��Center=�6�   {�6   �� $ IsSimulationRunning�$
 OneWay=�$ uc��# [  6�   �� $ UpdateUI�$
 TwoWay=�$ uc��.��6  	Update UI6  �� 5T   
   -   5        $��4��5T   �   $��3��6�   $��
0, 5, 0, 0q�6�   $��Center=�6�   ���#74cb83{�6   �� $ IsSimulationRunning�$
 OneWay=�$ uc��# [  5U      ��%_   '��       �� 5V      " ��6   .�5W      ��  �@��CornerRadius$ 10��65   5X      5Y      .��6    START BENCHMARK5\      �� 5^   
   -   5        $��5��5^   �   $��3��6�   $��
0, 5, 0, 0q�6�   $��Center=�6�   
���   �
�������{�6   �� $ IsSimulationRunning�$
 OneWay=�$ uc��# [  5_      ��%_   '��       �� 5`      " ��6   .�5a      ��  �@��CornerRadius$ 10��65   5b      5c      .��6    	RESTART5f      5g      5h      