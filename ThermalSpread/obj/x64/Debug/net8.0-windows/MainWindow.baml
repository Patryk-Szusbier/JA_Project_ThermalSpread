   M S B A M L   `   `   `  ����H  DThermalSpread, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ThermalSpreadThermalSpread  5   	   C&clr-namespace:ThermalSpread.componentsThermalSpread.components  5   	   C&clr-namespace:ThermalSpread.convertersThermalSpread.converters  5   	       ThermalSpread.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:ThermalSpread     5   	   =
components&clr-namespace:ThermalSpread.components     5   	   =
converters&clr-namespace:ThermalSpread.converters     5   	   -   5          � Title$  ThermalSpread��5   	   $	��600��6   $	��900��6,    � DataContext 5   	   ��  �� RelativeSource �� *Self+ � 	Resources 5      6   0ThermalSpread.converters.InverseBooleanConverter   InverseBooleanConverter9   3ThermalSpread.converters.BooleanVisibilityConverter  BooleanVisibilityConverter��%:   &	        &	        5   
   6-     5   
   60   5      .��5      � ���#e1e3f3 � ColumnDefinitions 5   
   �� 5      $��2* �6   �� 5      $��* �6   .+�5   
   �  � RowDefinitions 5      �� 5      $��3* �6    �� 5      $��* �6    .+�5      �� $��2��6   1   +ThermalSpread.components.TemperaturesCanvas  5      -   5          Name$ temperaturesCanvas��5       1   +ThermalSpread.components.TemperaturesCanvas   OnDrawingFinished$  TemperaturesCanvas_OnDrawn��5"         OnSizeChanged$&  TemperaturesCanvas_OnSizeChanged��5#      	   Gradient	 5!      �� 
 �� Path$
 Gradient�   InteractivityConfig 5$      �� $
 InteractivityConfig� �� Mode$ OneWay=�   IsInteractivityEnabled 5%      �� $
 IsAnySimulationRunning� �� 	Converter# [  $ OneWay=�,   &ThermalSpread.components.ConsoleOutput  5'      -   5          Name$ console��5'   4   $��2��6'   64   5(      �� 5)   
   ���#e1e3f3$��1��6*   .@�5+      �� $��
Simulation��6   .��5,      .   (ThermalSpread.components.SimulationPanel  .   (ThermalSpread.components.SimulationPanel   OnStart$ SimulationPanel_OnStart��51         OnStop$ SimulationPanel_OnStop��52         OnStartBenchmark$&  SimulationPanel_OnStartBenchmark��53         OnReset$ SimulationPanel_OnReset��54         UpdateUI 5-      �� $
 UpdateUI�$ TwoWay=�   	MinStepMs 5.      �� $
 	MinStepMs�$ TwoWay=�   NrOfThreads 5/      �� $
 NrOfThreads�$ TwoWay=�   SimulationTarget 50      �� $
 SimulationTarget�$ TwoWay=�   IsSimulationRunning 55      �� $
 IsAnySimulationRunning�$ OneWay=�57      �� 59      $��Interactivity��6   .��5:      1   +ThermalSpread.components.InteractivityPanel  1	   +ThermalSpread.components.InteractivityPanel 	  OnConfigChanged$( "InteractivityPanel_OnConfigChanged��5=       	  OnPresetSelected$) #InteractivityPanel_OnPresetSelected��5@         Gradient 5;      �� $
 Gradient�$ OneTime=�   	Thickness 5<      �� $#
 InteractivityConfig.Thickness�$ OneWay=�   WidthPx 5>      �� $
 Matrix.Config.Width�$ OneWay=�   HeightPx 5?      �� $
 Matrix.Config.Height�$ OneWay=�5@      5A      5B      5C      5D      