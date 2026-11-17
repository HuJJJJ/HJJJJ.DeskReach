#include "st7789_spi_lesson.h"

/* SET=高电平，RESET=低电平；这里的 RESET 不是复位整块芯片。 */
#define CS_L()   HAL_GPIO_WritePin(GPIOA, GPIO_PIN_4, GPIO_PIN_RESET)
#define CS_H()   HAL_GPIO_WritePin(GPIOA, GPIO_PIN_4, GPIO_PIN_SET)
#define SCL_L()  HAL_GPIO_WritePin(GPIOA, GPIO_PIN_5, GPIO_PIN_RESET)
#define SCL_H()  HAL_GPIO_WritePin(GPIOA, GPIO_PIN_5, GPIO_PIN_SET)
#define SDA_L()  HAL_GPIO_WritePin(GPIOA, GPIO_PIN_7, GPIO_PIN_RESET)
#define SDA_H()  HAL_GPIO_WritePin(GPIOA, GPIO_PIN_7, GPIO_PIN_SET)
#define DC_L()   HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_RESET)
#define DC_H()   HAL_GPIO_WritePin(GPIOB, GPIO_PIN_1, GPIO_PIN_SET)
#define RES_L()  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_RESET)
#define RES_H()  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0, GPIO_PIN_SET)

static void LCD_Wait(void)
{
    /* 故意放慢以便学习。HAL_Delay 单位为毫秒，不是微秒。
     * 实际等待取决于 HAL 时基，不能把这里当成精确 1 ms 波形。
     * 这版不适合刷全屏；理解时序后可换成定时延时或硬件 SPI。
     * 只能在 HAL 时基正常运行时调用，不要在中断里调用本驱动。
     */
    HAL_Delay(1);
}

void LCD_BusInit(void)
{
    GPIO_InitTypeDef gpio = {0};

    __HAL_RCC_GPIOA_CLK_ENABLE();
    __HAL_RCC_GPIOB_CLK_ENABLE();

    /* 先设置输出锁存值，再启用输出，减少上电时的意外脉冲。 */
    CS_H();
    SCL_L();
    SDA_L();
    DC_H();
    RES_H();

    gpio.Mode = GPIO_MODE_OUTPUT_PP; /* 普通推挽输出，手动控制电平 */
    gpio.Pull = GPIO_NOPULL;
    gpio.Speed = GPIO_SPEED_FREQ_HIGH; /* 引脚驱动速度，不是 SPI 时钟频率 */
    gpio.Pin = GPIO_PIN_4 | GPIO_PIN_5 | GPIO_PIN_7;
    HAL_GPIO_Init(GPIOA, &gpio);

    gpio.Pin = GPIO_PIN_0 | GPIO_PIN_1;
    HAL_GPIO_Init(GPIOB, &gpio);
    LCD_Wait();
}

void LCD_Reset(void)
{
    CS_H();
    SCL_L();
    RES_H();
    HAL_Delay(10);
    RES_L();                 /* RES 低有效，拉低让屏幕控制器复位 */
    HAL_Delay(20);
    RES_H();
    HAL_Delay(120);           /* 给复位恢复留出时间 */
}

/* 调用前必须 CS=0，且 DC 已经设置好。
 * 本函数只发 8 位，不负责判断这些位代表命令还是数据。
 */
static void LCD_SendByte(uint8_t value)
{
    uint8_t i;

    for (i = 0; i < 8; i++)
    {
        SCL_L();             /* 1. 时钟先保持低电平 */

        if (value & 0x80)    /* 0x80=10000000，检查当前最高位 */
            SDA_H();        /* 2. 本位为 1，把 SDA 置高 */
        else
            SDA_L();        /*    本位为 0，把 SDA 置低 */

        LCD_Wait();          /* 3. 提前摆稳数据：建立时间 TSDS */
        SCL_H();             /* 4. 上升沿！ST7789 在这里接收这一位 */
        LCD_Wait();          /* 5. 数据继续保持：覆盖 TSDH 和高脉宽 */
        SCL_L();             /* 6. 结束本次脉冲，回到低电平 */

        value <<= 1;         /* 7. 下一位移到最高位，供下一轮发送 */
    }
}

static void LCD_Begin(uint8_t is_data)
{
    SCL_L();
    if (is_data)
        DC_H();              /* 1：参数或像素数据 */
    else
        DC_L();              /* 0：命令 */

    CS_L();                  /* 选中屏幕，开始传输 */
    LCD_Wait();              /* 留出 CS 和 DC 的建立时间 */
}

static void LCD_End(void)
{
    LCD_Wait();              /* 最后一位之后，不立刻取消选中 */
    CS_H();                  /* 结束传输 */
    LCD_Wait();              /* 留出 CS 高电平间隔 TCHW */
}

void LCD_WriteCommand(uint8_t command)
{
    LCD_Begin(0);
    LCD_SendByte(command);
    LCD_End();
}

void LCD_WriteData(uint8_t data)
{
    LCD_Begin(1);
    LCD_SendByte(data);
    LCD_End();
}
