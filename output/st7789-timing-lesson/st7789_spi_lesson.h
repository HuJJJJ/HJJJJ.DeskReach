#ifndef ST7789_SPI_LESSON_H
#define ST7789_SPI_LESSON_H

#include "stm32f1xx_hal.h"

/* 学习用接线：PA4=CS, PA5=SCL, PA7=SDA, PB1=DC, PB0=RES。
 * 这是示例接线，不表示已经确认你的板子如此连接。
 * 用于带独立 DC 引脚的四线串行模块，只实现写入。
 */
void LCD_BusInit(void);
void LCD_Reset(void);
void LCD_WriteCommand(uint8_t command);
void LCD_WriteData(uint8_t data);

#endif
