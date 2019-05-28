Version 2.0
Strategy: simple strategy, not used.
Actions: no aimed actions.
Control: PD controller with relative positions.
Planning: event based planning (No periodic replanning).
Event detection: stuck event not handled.
Perception: simple estimation. constant angle, no energy losses impacts
Action Times: relative times - init is zeor final is some T (PointParams structure).
 -----------------------------------------------------------
Version 2.1
Planning: Independet planning thread, on demand (event based not periodic) strategy.
-----------------------------------------------------------
Version 2.2
Action Times: Absolute times, init is now, final is now plus some T (PointParams structure).
-----------------------------------------------------------
Version 2.3
Planning: periodic planning every fixed interval SEEMS TO WORK, sometimes miss the puck.
low level: now aware is its a new plan or a replan
-----------------------------------------------------------
Version 2.4
Planning: periodic planning every fixed interval works for internvals higher than 0.2 seconds only.
planning-period and move-time have been added to the constants structure.
Periodic planning is set only for attack type actions.
-----------------------------------------------------------
Version 2.5
Event Detection: better event detection, now detect stuck events (at the agent's side and the player's side).
Planning: plan for stuck situation at the agent's side. Both in ondemand planning and periodic planning.
-----------------------------------------------------------
Version 2.6
Actions: Complex actions are now implemented (Attack left/right/middle) in naive approach.
Replan period: 0.1 seconds




