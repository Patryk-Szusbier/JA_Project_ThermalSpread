   M S B A M L   `   `   `  ����H  DThermalSpread, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:ThermalSpread.componentsThermalSpread.components  5      (    "ThermalSpread.components.FileInput   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      8local&clr-namespace:ThermalSpread.components     5      -   5          >�Name$  uc��5      .��5	      � $��Stretch=�6    � RowDefinitions 5
   
   �� 5      $��14 �6   �� 5      $��20 �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��2��6   $��
0, 0, 2, 0q�6$   $��0q�68   $��10(�6D   $��Center=�6R   $��Right=�6m   ��6�   ��  �� Path$ 
InputLabel� �� Mode$ OneWay=� �� ElementName$ uc��� 5   
   $��1��6   $��0��6   $��2��6,   .+�5      �� -   5         ��Name$ textBox��5      $��12(�6   $��Center=�6F   $��Stretch=�6a   ��6�   �� $ FilePath�$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�$ uc���� 5      $��12(�6    J� IsHitTestVisible$ False��6&   $��Center=�6}   $
��Left=�6�   $��10,0,0,0q�6�   
���������6?   �� $ 
InputLabel�$ OneWay=�$ uc����5      �� 5      	 �� 
TargetType"	 ��6   .�5      �� 
 �� Property
 � y� �� Value$ 	Collapsed=�67    �� Triggers 5      �� 5       �� Binding 6*   �� *Text+$ textBox�� �� Value  6X   .a�5   "   �� 
 � y�$ Visible=�6?   5      5!      5#      5$      �� 5&   
   -   5        $��2��5&      $��0��6   $��
0, 1, 5, 0q�6.   $��0, -1.5, 0, 0q�6B   $��12(�6{   .��6�   OPEN EXPLORER6�   �� 5'   
   -   5        $��0, -1, 0, 0q�5'   S   $��2��6i   $��1��6v   $��
0, 1, 0, 0q�6�   {�6   �� $ 	IsCorrect�$ OneWay=�$ uc��.��5(      �� $��/images/save-icon.png��6   $��15��63   $��15��6>   5)      5*      5+      