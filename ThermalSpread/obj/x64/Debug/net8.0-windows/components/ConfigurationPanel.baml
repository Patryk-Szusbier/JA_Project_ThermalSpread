   M S B A M L   `   `   `  ����H  DThermalSpread, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:ThermalSpread.componentsThermalSpread.components  5      1    +ThermalSpread.components.ConfigurationPanel   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      8local&clr-namespace:ThermalSpread.components     5      -   5          >�Name$  uc��5      .��5
      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $	��100 �6   �� 5      $��* �6   .+�5   
   �� $
��Wrap=�6   $��0��6(   $��Center=�65   ��5      `� 5       �� StringFormat$ Alfa ={0:N2}��6   .1�5      ��  �� Path$ 	AlfaCoeff�6    �� ElementName$ uc��6/    �� Mode$ OneWay=�6@   5      5      �� 5   
   -   5        $��0��5   4   $��1��6A   $��2��6Q   $��Center=�6e    � Minimum$	 -10Q�6�    � Maximum$ 10Q�6�   	 �� TickFrequency$	 1Q�6�   
 �� IsSnapToTickEnabled$

 True��6�    � Value 6�   �� $ 	AlfaCoeff�$ TwoWay=�$ uc��(   "ThermalSpread.components.FileInput  5   
   (   "ThermalSpread.components.FileInput   OnAction$ ConfigFileLoad_OnAction��6r   $��1��6   $��2��6'      
InputLabel$ Config File Load��6;      FileExtension$ HeatConf��6Y   6r     5!   
   $ ConfigFileWrite_OnAction��6s   $��2��6   $��2��6'   $ Config File Write��6;   $ HeatConf��6Z   6s   5#      5$      