   M S B A M L   `   `   `  ���� D  @Presentor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:Presentor	Presentor      Presentor.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     )localclr-namespace:Presentor     _ [MaterialDesignThemes.Wpf, Version=4.4.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  X TXceed.Wpf.Toolkit, Version=4.3.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f47toolkit)http://schemas.xceed.com/wpf/xaml/toolkit  -     � Title$  Office Ware Pressentor�� � WindowState$ 	Maximized=�.��]� .+��� $	��Top=�$	��125��.@��� $��Файл��.����  �� Orientation$ 
Horizontal=�.+��� -   $��50 15q�.+�'  !MaterialDesignThemes.Wpf.PackIcon  $��Center=�   Kind$ 	OpenInApp=�$��48��$��48���� $��Center=�$+��%Открыть презентацию���� -   $��50 15q�.+�  $��Center=�$ ContentSave=�$��48��$��48���� $��Center=�$/��)Сохранить презентацию���� -   $��50 15q�.+�  $��Center=�$ Close=�$��48��$��48���� $��Center=�$+��%Закрыть презентацию���� $��Слайды��.���� $ 
Horizontal=�.+��� -   $��50 15q�.+�  $��Center=�$	 Add=�$��48��$��48���� $��Center=�$��Создать слайд���� -   $��50 15q�.+�  $��Center=�$
 Play=�$��48��$��48���� $��Center=�$1��+Воспроизвести анимацию���� $��50 15q�.+�#  Xceed.Wpf.Toolkit.ColorPicker  -     Name$ backgroundPicker��$��Center=�$��48��$��24��$
��0 10q��� $��Center=�$&�� Задать фон слайда���� $��50 15q�.+�&   Xceed.Wpf.Toolkit.TimeSpanUpDown  -     Name$ durationPicker��,  &Xceed.Wpf.Toolkit.Primitives.InputBase   AllowTextInput .  �  �Xceed.Wpf.Toolkit.Primitives.UpDownBase`1[[System.Nullable`1[[System.TimeSpan, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   Minimum$ 01:00X�   Value$ 01:00X�	   Maximum$	 15:00X�
   ShowSeconds
 .     ShowDays .  $��0 10 0 0q��� $��Center=�$B��<Задать продолжительность слайда��$��0 10 0 0q��� $��Вставка��.���� $ 
Horizontal=�.+��� -	   $��50 15q� � DataContext$' !Текстовая область��.+�  $��Center=�$ TextBox=�$��48��$��48���� $��Center=�$'��!Текстовая область���� -
   $��50 15q�$ Картинка��.+�  $��Center=�$ Image=�$��48��$��48���� $��Center=�$��Картинка���� $��Анимация��.���� $ 
Horizontal=�.+��� -   $��50 15q�.+�  $��Center=�$ 	Animation=�$��48��$��48���� $��Center=�$��Анимация��]� 
�������$��Bottom=�$��25��.+��� $	��350��$
��Left=�$ 
Horizontal=�.+��� -    ��Name$ slidesStatsLabel��$��5q�$��Слайд 1 из 1���� $	��350��$��Right=�$ 
Horizontal=�.+�  -   $��5q�$ 	Slideshow=�$��Right=��� $
��Left=�.���� -    ��Name$ slides��
�������$	��250�� � ContextMenu �� .@�m� -   $��Создать слайд��.+��� -   $	��100��$��25q�
�����p�$��5������ ]� $��Right=�.+��� -   $ animationBar��$��Right=�$	��325��
�������$y�	Collapsed=�.+��� $��15q�$F��@Продолжительность анимации начала��  -   $" startAnimationDurationPicker�� .  $ 01:00X�$ 01:00X�$	 15:00X�
 .   .  $
��25 5q�$ start���� $��15q�$D��>Продолжительность анимации конца��  -   $  endAnimationDurationPicker�� .  $ 01:00X�$ 01:00X�$	 15:00X�
 .   .  $
��25 5q�$	 end���� -    ��Name$ slideControl�� � Cursor$	 Pen��$
��Left=�.@��� $y�	Collapsed=�.���� -    ��Name$ activeSlide��
�������