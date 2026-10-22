# HP-250 Hydraulic Press – Operation and Maintenance Manual

Document: MAN-HP250-EN | Revision: 3 | Date: 2025-11-14 | Equipment: hydraulic press HP-250, press line P-12

## 1 Safety

### 1.1 General rules

The HP-250 may only be operated by persons who have completed the press operator training and are listed in the line qualification matrix. Maintenance work may only be carried out by authorized maintenance technicians.

Required personal protective equipment at the press: safety glasses, cut-resistant gloves (level C), safety shoes S3 and hearing protection. Hearing protection is mandatory because the noise level at the operator position exceeds 85 dB(A) in continuous mode.

### 1.2 Safety devices

- Two-hand control on the operator console for single stroke mode.
- Type 4 light curtain on the front side, resolution 30 mm, minimum safety distance 450 mm.
- Three emergency stop buttons: front console, rear side and main control panel.
- Interlocked side and rear guards. Opening a guard stops the slide immediately.

Safety devices must never be bypassed or modified. The operator tests the light curtain and all emergency stop buttons at the start of every shift and records the test in the shift checklist.

### 1.3 Residual risks

The hydraulic system contains stored energy. The accumulator (4 L, pre-charge pressure 90 bar) keeps pressure even after the pump is switched off. Before any work inside the press or on the hydraulic system, the accumulator must be discharged and the slide must rest on safety blocks. Follow the lockout/tagout procedure LOTO-001.

## 2 Technical data

| Parameter | Value |
|---|---|
| Nominal force | 2500 kN |
| Stroke | 600 mm |
| Maximum working pressure | 280 bar |
| Approach speed | 350 mm/s |
| Pressing speed | 15–40 mm/s |
| Return speed | 300 mm/s |
| Bolster area | 1800 × 1200 mm |
| Maximum die weight | 6 t |
| Hydraulic oil | HLP ISO VG 46, tank volume 900 L |
| Main motor | 55 kW |
| Normal oil temperature | 35–55 °C |
| Compressed air supply | 6 bar |

## 3 Controls and operating modes

### 3.1 Control panel

The main control panel has a touch screen (HMI), a mode selector, a key switch for setup mode and the main emergency stop. The HMI shows the current mode, oil temperature, working pressure, stroke counter and active alarms.

### 3.2 Setup mode

Setup mode is used only for die change and die adjustment. It is selected with the key switch. In setup mode the slide moves at a maximum of 10 mm/s and only while the hold-to-run button is pressed. Automatic feeding is disabled.

### 3.3 Single stroke mode

In single stroke mode the slide performs one complete stroke after the operator presses both buttons of the two-hand control within 0.5 s. Releasing one button during the downstroke stops the slide.

### 3.4 Continuous mode

Continuous mode is allowed only with the automatic feeder connected and the light curtain active. The stroke rate is set on the HMI (maximum 18 strokes per minute). Any interruption of the light curtain stops the press at the next top dead center.

## 4 Start-up and shutdown

### 4.1 Checks before start-up

1. Oil level is between MIN and MAX on the sight glass of the tank.
2. No oil leaks under the press or on the hose connections.
3. All guards are closed and undamaged.
4. Compressed air pressure is at least 6 bar.
5. Light curtain and emergency stop test is passed and recorded.

### 4.2 Start-up

Switch on the main switch Q0, reset the safety relay on the HMI and start the hydraulic pump. If the oil temperature is below 25 °C, let the pump run idle for 10 minutes so that the oil heater and circulation bring the oil to operating temperature. Production may start only when the HMI shows no active alarm.

### 4.3 Shutdown

Move the slide to top dead center, stop the hydraulic pump and switch off the main switch. At the end of the week, or before a stop longer than 24 hours, lower the slide onto the safety blocks.

## 5 Die change

The HP-250 is equipped with hydraulic die clamps (clamping pressure 250 bar) and a roller conveyor for quick die change. The target die change time on line P-12 is 20 minutes. The detailed step-by-step procedure is described in SOP-014 "Die change on press line P-12". Die change is always done in setup mode.

## 6 Maintenance schedule

| Interval | Task | Responsible |
|---|---|---|
| Every shift | Check oil level and leaks; test light curtain and emergency stops | Operator |
| Weekly | Clean bolster and slide surfaces; inspect die clamp hoses | Operator |
| Monthly | Check grease reservoir of central lubrication; inspect slide gibs | Maintenance |
| Every 1000 operating hours | Replace the return filter element (10 µm) | Maintenance |
| Every 4000 operating hours or once a year | Change hydraulic oil, take an oil sample for analysis, replace the breather filter | Maintenance |
| Once a year | Safety inspection by an authorized person; check accumulator pre-charge pressure (90 bar) | Maintenance and safety officer |

## 7 Lubrication

The slide gibs are lubricated by a central lubrication pump with grease NLGI 2 EP. The pump delivers 12 pulses per hour during operation. The grease reservoir holds 2 kg. Refill the reservoir when the level drops below 25 %; the HMI shows warning W020 at this level. Use only the grease specified on the reservoir label.

## 8 Hydraulic oil change

1. Apply lockout/tagout according to LOTO-001 and discharge the accumulator.
2. Place a collecting tray and drain the oil through drain valve V-7.
3. Replace the return filter element and the breather filter.
4. Clean the tank through the inspection cover if sludge is visible.
5. Refill with 900 L of HLP ISO VG 46 through the filter cart (10 µm). Never pour oil directly into the tank.
6. Bleed the system, then run 10 strokes in setup mode.
7. Check the oil level again and top up to MAX.

Target oil cleanliness after the oil change is ISO 4406 class 18/16/13. Record the oil change and the sample number in the maintenance system.

## 9 Alarms and troubleshooting

| Code | Message | Probable cause | Action |
|---|---|---|---|
| E101 | Emergency stop active | Emergency stop pressed or safety circuit open | Release the emergency stop, check the circuit, reset the safety relay |
| E107 | Light curtain interrupted | Object in the protected field or dirty lenses | Remove the object, clean the lenses, reset |
| E214 | Oil temperature high (above 60 °C) | Cooler fault, low oil level or clogged cooler | Stop production; check cooling water flow (minimum 40 L/min) and oil level; restart only below 55 °C |
| E215 | Oil temperature low (below 20 °C) | Cold start | Run the 10-minute warm-up |
| E230 | Return filter clogged | Differential pressure above 3.5 bar | Replace the filter element and check oil cleanliness |
| E305 | Tonnage overload | Wrong die shut height or double blank | Stop; check die shut height and blank feeding. Do not reset more than once without the shift supervisor |
| E412 | Die clamp pressure low | Leak in a clamp hose or clamp pump fault | Do not run the press; check the clamps and call maintenance |
| W020 | Lubrication reservoir low | Grease level below 25 % | Refill grease NLGI 2 EP |

## 10 Contacts

Maintenance hotline: extension 4410. Shift supervisor: extension 4400. Production engineering (die change and SOP questions): extension 4420.
